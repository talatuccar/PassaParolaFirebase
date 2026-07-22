using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;
using Passaparola.Multiplayer;
using Passaparola.MainMenu;

public class FirebaseNetworkAdapter : MonoBehaviour, IRealtimeNetworkAdapter
{
    private const string BaseUrl = "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/";
    private string _currentRoomId;
    private string _localPlayerId;

    public string CurrentRoomId => _currentRoomId;
    public string LocalPlayerId => _localPlayerId;

    private int _lastHandledQuestionIndex = -1;
    private string _lastHandledStatus = "";

    public event Action<int, AnswerStatus> OnOpponentAnswered;

    private void Awake()
    {
        _localPlayerId = "player_" + UnityEngine.Random.Range(1000, 9999) + "_" + Guid.NewGuid().ToString().Substring(0, 5);
    }

    public void ConnectAndFindMatch(Action<string> onMatchFound)
    {
        GameMode mode = MainMenuUIController.SelectedGameMode;
        string roomCode = MainMenuUIController.CurrentRoomCode;

        if (mode == GameMode.FriendRoom && !string.IsNullOrEmpty(roomCode))
        {
            // --- ARKADAÞINLA OYNA (Deðiþmedi, zaten çalýþýyor) ---
            _currentRoomId = "room_private_" + roomCode;
            StartCoroutine(CheckAndJoinRoom(onMatchFound));
        }
        else
        {
            // --- RASTGELE DÜELLO (Yeni Dinamik Matchmaking) ---
            StartCoroutine(FindOrCreateRandomDuelRoom(onMatchFound));
        }
    }

    /// <summary>
    /// Rastgele Düello için bekleyen 1 kiþilik oda arar. Bulursa girer, bulamazsa yeni oda kurar.
    /// </summary>
    private IEnumerator FindOrCreateRandomDuelRoom(Action<string> onMatchFound)
    {
        string roomsUrl = $"{BaseUrl}rooms.json";

        using (UnityWebRequest req = UnityWebRequest.Get(roomsUrl))
        {
            yield return req.SendWebRequest();

            string foundRoomId = null;

            if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler.text) && req.downloadHandler.text != "null")
            {
                var roomsData = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;

                if (roomsData != null)
                {
                    // Odalarý tara: Sadece 'room_random_duel_' ile baþlayan ve içinde tam 1 oyuncu olan odayý bul
                    foreach (var room in roomsData)
                    {
                        if (room.Key.StartsWith("room_random_duel_"))
                        {
                            var roomDict = room.Value as Dictionary<string, object>;
                            if (roomDict != null && roomDict.ContainsKey("players"))
                            {
                                var players = roomDict["players"] as Dictionary<string, object>;
                                if (players != null && players.Count == 1) // Tam 1 kiþi bekliyor!
                                {
                                    foundRoomId = room.Key;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (foundRoomId != null)
            {
                // Boþ yer olan odaya katlanýyoruz
                _currentRoomId = foundRoomId;
                Debug.Log($"[MATCHMAKING] Bekleyen odaya katýldýk: {_currentRoomId}");
            }
            else
            {
                // Bekleyen oda yoksa yepyeni benzersiz bir oda oluþturuyoruz
                _currentRoomId = "room_random_duel_" + UnityEngine.Random.Range(1000, 9999);
                Debug.Log($"[MATCHMAKING] Yeni rastgele oda oluþturuldu: {_currentRoomId}");
            }

            StartCoroutine(RegisterAndWaitForOpponent(onMatchFound));
        }
    }

    /// <summary>
    /// Katýlmak istenen özel oda var mý kontrol eder, yoksa oyuna sokmaz! (Arkadaþ Modu Ýçin)
    /// </summary>
    /// <summary>
    /// Katýlmak istenen özel oda var mý VE oda müsait mi (2. kiþi dolmamýþ mý) kontrol eder!
    /// </summary>
    private IEnumerator CheckAndJoinRoom(Action<string> onMatchFound)
    {
        bool isHost = MainMenuUIController.IsHost;

        if (!isHost)
        {
            // 1. Oda Firebase'de Var Mý ve Dolu Mu Sorgula
            string checkRoomUrl = $"{BaseUrl}rooms/{_currentRoomId}.json";

            using (UnityWebRequest req = UnityWebRequest.Get(checkRoomUrl))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success || string.IsNullOrEmpty(req.downloadHandler.text) || req.downloadHandler.text == "null")
                {
                    Debug.LogError($"[NETWORK HATA] {MainMenuUIController.CurrentRoomCode} kodlu oda bulunamadý!");
                    StopAllCoroutines();
                    yield break;
                }

                // --- YENÝ KONTROL: Oda var ama dolu mu? ---
                var roomData = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
                if (roomData != null && roomData.ContainsKey("players"))
                {
                    var players = roomData["players"] as Dictionary<string, object>;
                    if (players != null && players.Count >= 2)
                    {
                        Debug.LogError($"[NETWORK HATA] {MainMenuUIController.CurrentRoomCode} kodlu oda zaten DOLU!");
                        StopAllCoroutines();
                        yield break; // 3. oyuncunun girmesini engelle!
                    }
                }
            }
        }

        StartCoroutine(RegisterAndWaitForOpponent(onMatchFound));
    }
    private IEnumerator RegisterAndWaitForOpponent(Action<string> onMatchFound)
    {
        // 1. Kendi adýmýzý odanýn altýna kaydet
        string url = $"{BaseUrl}rooms/{_currentRoomId}/players/{_localPlayerId}/name.json";
        string playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(100, 999));
        string jsonBody = $"\"{playerName}\"";

        using (UnityWebRequest req = UnityWebRequest.Put(url, jsonBody))
        {
            req.method = "PUT";
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
        }

        Debug.Log($"[NETWORK] Odaya baþarýyla girildi ({_currentRoomId}). Rakip bekleniyor...");

        // 2. Oyuncu sayýsý 2 olana kadar bekle
        bool isMatchReady = false;

        while (!isMatchReady)
        {
            yield return new WaitForSeconds(0.5f);

            string checkUrl = $"{BaseUrl}rooms/{_currentRoomId}/players.json";

            using (UnityWebRequest req = UnityWebRequest.Get(checkUrl))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler.text))
                {
                    var playersData = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;

                    if (playersData != null && playersData.Count >= 2)
                    {
                        isMatchReady = true;
                        Debug.Log("[NETWORK] Odada 2 oyuncu var! Maç Baþlýyor...");
                    }
                }
            }
        }

        onMatchFound?.Invoke(_currentRoomId);
        StartCoroutine(PollOpponentAnswers());
    }

    public void SendMyAnswer(int questionIndex, AnswerStatus status)
    {
        StartCoroutine(SendAnswerRoutine(questionIndex, status));
    }

    private IEnumerator SendAnswerRoutine(int questionIndex, AnswerStatus status)
    {
        string url = $"{BaseUrl}rooms/{_currentRoomId}/players/{_localPlayerId}/lastAnswer.json";
        string jsonBody = $"{{\"index\":{questionIndex},\"status\":\"{status}\"}}";

        using (UnityWebRequest req = UnityWebRequest.Put(url, jsonBody))
        {
            req.method = "PUT";
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
        }
    }

    private IEnumerator PollOpponentAnswers()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);

            string url = $"{BaseUrl}rooms/{_currentRoomId}/players.json";

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler.text))
                {
                    ParseOpponentData(req.downloadHandler.text);
                }
            }
        }
    }

    private void ParseOpponentData(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText) || jsonText == "null") return;

        var playersData = Json.Deserialize(jsonText) as Dictionary<string, object>;
        if (playersData == null) return;

        foreach (var playerPair in playersData)
        {
            string playerId = playerPair.Key;

            if (playerId != _localPlayerId)
            {
                var playerObj = playerPair.Value as Dictionary<string, object>;
                if (playerObj != null && playerObj.ContainsKey("lastAnswer"))
                {
                    var lastAnswer = playerObj["lastAnswer"] as Dictionary<string, object>;
                    if (lastAnswer != null)
                    {
                        int qIndex = Convert.ToInt32(lastAnswer["index"]);
                        string statusStr = lastAnswer["status"].ToString();

                        if (qIndex != _lastHandledQuestionIndex || statusStr != _lastHandledStatus)
                        {
                            _lastHandledQuestionIndex = qIndex;
                            _lastHandledStatus = statusStr;

                            if (Enum.TryParse(statusStr, out AnswerStatus status))
                            {
                                OnOpponentAnswered?.Invoke(qIndex, status);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ListenToOpponentAnswers(Action<int, AnswerStatus> onOpponentAnswered)
    {
        OnOpponentAnswered += onOpponentAnswered;
    }
}
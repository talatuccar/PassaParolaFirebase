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
            _currentRoomId = "room_private_" + roomCode;

            // Eðer oyuncu katýlan (Join) tarafsa önce odayý kontrol et
            // Note: MainMenu'den 'IsRoomHost' gibi bir bool alabiliriz veya direkt odayý sorgulayabiliriz.
            StartCoroutine(CheckAndJoinRoom(onMatchFound));
        }
        else
        {
            // Rastgele Düello
            _currentRoomId = "room_random_duel";
            StartCoroutine(RegisterAndWaitForOpponent(onMatchFound));
        }
    }

    /// <summary>
    /// Katýlmak istenen oda var mý kontrol eder, yoksa oyuna sokmaz!
    /// </summary>
    private IEnumerator CheckAndJoinRoom(Action<string> onMatchFound)
    {
        bool isHost = MainMenuUIController.IsHost; // Ana menüde odayý kuran kiþi mi katýlan kiþi mi?

        if (!isHost)
        {
            // 1. Oda Firebase'de Var Mý Sorgula
            string checkRoomUrl = $"{BaseUrl}rooms/{_currentRoomId}.json";

            using (UnityWebRequest req = UnityWebRequest.Get(checkRoomUrl))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success || string.IsNullOrEmpty(req.downloadHandler.text) || req.downloadHandler.text == "null")
                {
                    Debug.LogError($"[NETWORK HATA] {MainMenuUIController.CurrentRoomCode} kodlu oda bulunamadý!");

                    StopAllCoroutines();
                    // UI tarafýnda uyarý vermek için callback çaðrýlabilir veya log basýlabilir
                    yield break; // ÝÞLEMÝ BURADA KES, BEKLEMEYE GEÇME!
                }
            }
        }

        // Oda varsa veya Odayý Kuran Kiþiysek odaya kaydol ve rakip bekle
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
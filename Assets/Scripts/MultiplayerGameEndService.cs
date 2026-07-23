using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;

public class MultiplayerGameEndService : MonoBehaviour
{
    private const string BaseUrl = "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/";

    /// <summary>
    /// Lokal oyuncunun skorunu Firebase'e gönderir.
    /// </summary>
    public void SendFinalScore(string roomId, string playerId, PlayerDataSo playerData)
    {
        StartCoroutine(SendFinalScoreRoutine(roomId, playerId, playerData));
    }

    private IEnumerator SendFinalScoreRoutine(string roomId, string playerId, PlayerDataSo playerData)
    {
        string url = $"{BaseUrl}rooms/{roomId}/players/{playerId}/finalScore.json";
        string jsonBody = $"{{\"correct\":{playerData.playerCorrect},\"wrong\":{playerData.playerWrong},\"passed\":{playerData.playerPassed},\"score\":{playerData.playerScore},\"name\":\"{playerData.playerName}\"}}";

        using (UnityWebRequest req = UnityWebRequest.Put(url, jsonBody))
        {
            req.method = "PUT";
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
        }
    }

    /// <summary>
    /// Rakip oyuncunun final skorunu atmasýný bekler ve veriyi döndürür.
    /// </summary>
    public IEnumerator WaitForOpponentFinalScore(string roomId, string localPlayerId, Action<PlayerDataSo> onOpponentDataReceived)
    {
        bool received = false;

        while (!received)
        {
            yield return new WaitForSeconds(0.5f);

            string url = $"{BaseUrl}rooms/{roomId}/players.json";

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler.text) && req.downloadHandler.text != "null")
                {
                    var playersData = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
                    if (playersData != null)
                    {
                        foreach (var pair in playersData)
                        {
                            if (pair.Key != localPlayerId) // Rakip oyuncu
                            {
                                var playerObj = pair.Value as Dictionary<string, object>;
                                if (playerObj != null && playerObj.ContainsKey("finalScore"))
                                {
                                    var scoreDict = playerObj["finalScore"] as Dictionary<string, object>;
                                    if (scoreDict != null)
                                    {
                                        received = true;

                                        // ScriptableObject veya geçici nesneye veriyi basýyoruz
                                        PlayerDataSo opponentSo = ScriptableObject.CreateInstance<PlayerDataSo>();
                                        opponentSo.playerName = scoreDict["name"].ToString();
                                        opponentSo.playerCorrect = Convert.ToInt32(scoreDict["correct"]);
                                        opponentSo.playerWrong = Convert.ToInt32(scoreDict["wrong"]);
                                        opponentSo.playerPassed = Convert.ToInt32(scoreDict["passed"]);
                                        opponentSo.playerScore = Convert.ToInt32(scoreDict["score"]);

                                        onOpponentDataReceived?.Invoke(opponentSo);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void LeaveRoomAndCleanUp(string roomId, System.Action onComplete)
    {
        StartCoroutine(LeaveRoomRoutine(roomId, onComplete));
    }

    private IEnumerator LeaveRoomRoutine(string roomId, System.Action onComplete)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            onComplete?.Invoke();
            yield break;
        }

        string leftCountUrl = $"{BaseUrl}rooms/{roomId}/leftCount.json";
        int currentLeftCount = 0;

        // 1. ADIM: Mevcut leftCount deðerini Firebase'den oku
        using (UnityWebRequest getReq = UnityWebRequest.Get(leftCountUrl))
        {
            yield return getReq.SendWebRequest();

            if (getReq.result == UnityWebRequest.Result.Success &&
                !string.IsNullOrEmpty(getReq.downloadHandler.text) &&
                getReq.downloadHandler.text != "null")
            {
                int.TryParse(getReq.downloadHandler.text, out currentLeftCount);
            }
        }

        currentLeftCount++;

        // 2. ADIM: Eðer 2. kiþi de çýktýysa ODAYI SÝL, ilk kiþiyse leftCount = 1 yap
        if (currentLeftCount >= 2)
        {
            string deleteRoomUrl = $"{BaseUrl}rooms/{roomId}.json";
            using (UnityWebRequest deleteReq = UnityWebRequest.Delete(deleteRoomUrl))
            {
                yield return deleteReq.SendWebRequest();
                Debug.Log($"[NETWORK] 2. Oyuncu da çýktý. Oda tamamen silindi: {roomId}");
            }
        }
        else
        {
            // Firebase Realtime DB'ye sadece "1" string/int verisi gönderiyoruz
            using (UnityWebRequest putReq = UnityWebRequest.Put(leftCountUrl, "1"))
            {
                putReq.method = "PUT";
                putReq.SetRequestHeader("Content-Type", "application/json");
                yield return putReq.SendWebRequest();
                Debug.Log($"[NETWORK] Ýlk oyuncu çýktý. leftCount 1 olarak güncellendi.");
            }
        }

        // 3. ADIM: Firebase iþlemi %100 bitti! Artýk sahneyi güvenle deðiþtirebiliriz.
        onComplete?.Invoke();
    }
}
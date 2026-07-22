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
}
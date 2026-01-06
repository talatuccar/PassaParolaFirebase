using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    private const string LeaderboardKey = "passa_leaderboard";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLeaderboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddNewEntry(string name, int score)
    {
        leaderboard.Add(new LeaderboardEntry(name, score));
        leaderboard = leaderboard.OrderByDescending(e => e.score).Take(5).ToList();
        SaveLeaderboard();
    }
    public void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new LeaderboardWrapper(leaderboard));
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }
    public void LoadLeaderboard()
    {
        if (PlayerPrefs.HasKey(LeaderboardKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardKey);
            leaderboard = JsonUtility.FromJson<LeaderboardWrapper>(json).entries;
        }
    }

    [Serializable]
    private class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        public LeaderboardWrapper(List<LeaderboardEntry> list)
        {
            entries = list;
        }
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public LeaderboardEntry(string name, int score)
        {
            playerName = name;
            this.score = score;
        }
    }
}

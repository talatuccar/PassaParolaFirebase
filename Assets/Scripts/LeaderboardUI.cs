using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public Transform entryParent; 
    public GameObject entryPrefab;
    public Button closeButton;


    private void Start()
    {
        closeButton.onClick.AddListener(HideLeaderBoard);
    }
    public void ShowLeaderboard()
    {
        foreach (Transform child in entryParent)
        {
            Destroy(child.gameObject); 
        }

        var entries = LeaderboardManager.Instance.leaderboard;
        int index = 1;
        foreach (var entry in entries)
        {
            GameObject go = Instantiate(entryPrefab, entryParent);
            go.GetComponentInChildren<TMP_Text>().text = index + ". " + entry.playerName + ": " + entry.score;
            index++;
        }
        entryParent.transform.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    public void HideLeaderBoard()
    {
        closeButton.gameObject.SetActive(false);
        entryParent.transform.gameObject.SetActive(false);
    }
}

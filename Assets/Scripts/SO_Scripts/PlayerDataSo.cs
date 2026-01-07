
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerDataSO", order = 1)]
public class PlayerDataSo : ScriptableObject
{
    [Header("Player Data")]
    public string playerName;
    public int playerScore;
    public int playerCorrect;
    public int playerWrong;
    public int playerPassed;
    private void OnEnable()
    {
        playerScore = 0;
        playerCorrect = 0;
        playerWrong = 0;
        playerPassed = 0;
        playerName = string.Empty;
    }
    public int GetScore(int correct, int wrong, int passed)
    {
        GetData(correct, wrong, passed);

        int score = correct * 10 - wrong * 5 - passed * 2;

        playerScore = score;

        return playerScore;
    }
    void GetData(int correct, int wrong, int passed)
    {
        playerCorrect = correct;
        playerWrong = wrong;
        playerPassed = passed;
    }
}

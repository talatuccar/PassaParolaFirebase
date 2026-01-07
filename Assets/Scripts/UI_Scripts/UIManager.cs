
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanelPrefab;
    [SerializeField] private GameObject gameOverPanelParent;

    public PlayerDataSo[] playerData;
    public GameDataSo gameDataSo;
    public GameObject gameOverButtonPanel;
    public void HideGameOverPanel()
    {
        gameOverPanelPrefab.SetActive(false);
    }
    public void InstantiatePanel()
    {
        int highestScore = int.MinValue;
        int sameScoreCount = 0;

        foreach (PlayerDataSo gameDataSo in playerData)
        {
            if (gameDataSo.playerScore > highestScore)
            {
                highestScore = gameDataSo.playerScore;

            }
            else if (gameDataSo.playerScore == highestScore)
            {
                sameScoreCount++;
            }
        }

        bool isDraw = sameScoreCount > 0;

        foreach (PlayerDataSo playerDataSo in playerData)
        {
            GameObject panelObj = Instantiate(gameOverPanelPrefab, gameOverPanelParent.transform);

            Image panelImage = panelObj.GetComponent<Image>();
            if (panelImage != null)
            {
                if (isDraw)
                {
                    panelImage.color = Color.gray; 
                }
                else
                {
                    panelImage.color = (playerDataSo.playerScore == highestScore) ? Color.green : Color.red;

                    if (playerDataSo.playerScore == highestScore)
                    {
                        string winnerName = playerDataSo.playerName;
                        int winnerScore = playerDataSo.playerScore;
                        LeaderboardManager.Instance.AddNewEntry(winnerName, winnerScore);
                    }

                }
            }

            GameOverPanelUI panelUI = panelObj.GetComponent<GameOverPanelUI>();
            if (panelUI != null)
            {
                panelUI.SetValues(
                    playerDataSo.playerCorrect,
                    playerDataSo.playerWrong,
                    playerDataSo.playerPassed,
                    playerDataSo.playerScore,
                    playerDataSo.playerName
                );
            }
        }

        gameDataSo.ResetData();
        gameOverButtonPanel.gameObject.SetActive(true);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

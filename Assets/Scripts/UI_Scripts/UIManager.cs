using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanelPrefab;
    [SerializeField] private GameObject gameOverPanelParent;
    [SerializeField] private MultiplayerGameEndService gameEndService;
    [SerializeField] private Button mainMenuButton;
    public PlayerDataSo[] playerData;
    public GameDataSo gameDataSo;
    public GameObject gameOverButtonPanel;
    private IRealtimeNetworkAdapter networkAdapter;
    public void HideGameOverPanel()
    {
        if (gameOverPanelPrefab != null)
            gameOverPanelPrefab.SetActive(false);
    }

    void Awake()
    {
        networkAdapter = FindFirstObjectByType<FirebaseNetworkAdapter>();
    }

    public void InstantiatePanel()
    {
        int highestScore = int.MinValue;

        // 1. En yüksek skoru bul
        foreach (PlayerDataSo data in playerData)
        {
            if (data.playerScore > highestScore)
            {
                highestScore = data.playerScore;
            }
        }

        // 2. En yüksek skora sahip kaç oyuncu var say
        int highestScoreCount = 0;
        foreach (PlayerDataSo data in playerData)
        {
            if (data.playerScore == highestScore)
            {
                highestScoreCount++;
            }
        }

        // Eðer 1'den fazla kiþi en yüksek skoru aldýysa beraberedir
        bool isDraw = highestScoreCount > 1;

        // 3. Panelleri oluþtur ve UI elemanlarýný doldur
        foreach (PlayerDataSo playerDataSo in playerData)
        {
            GameObject panelObj = Instantiate(gameOverPanelPrefab, gameOverPanelParent.transform);

            Image panelImage = panelObj.GetComponent<Image>();
            if (panelImage != null)
            {
                if (isDraw)
                {
                    panelImage.color = Color.gray; // Berabere
                }
                else
                {
                    bool isWinner = (playerDataSo.playerScore == highestScore);
                    panelImage.color = isWinner ? Color.green : Color.red;

                    if (isWinner)
                    {
                        LeaderboardManager.Instance.AddNewEntry(playerDataSo.playerName, playerDataSo.playerScore);
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
        gameOverPanelParent.SetActive(true); 
        if (gameDataSo != null) gameDataSo.ResetData();
        if (gameOverButtonPanel != null) gameOverButtonPanel.SetActive(true);
    }

    public void OnMainMenuButtonClicked()
    {
        if (mainMenuButton != null) mainMenuButton.interactable = false;

        gameEndService.LeaveRoomAndCleanUp(networkAdapter.CurrentRoomId, () =>
        {     
            SceneManager.LoadScene("Menu");
        });
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
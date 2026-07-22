using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace Passaparola.MainMenu
{
    public enum GameMode
    {
        RandomDuel,  // 1v1 Canlý Düello
        FriendRoom   // Arkadaþýnla Oyna
    }

    public class MainMenuUIController : MonoBehaviour
    {
        [Header("Main Mode Buttons")]
        [SerializeField] private Button randomDuelButton;
        [SerializeField] private Button friendRoomButton;

        [Header("Room Panel Components (Arkadaþýnla Oyna Ýçin)")]
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private TMP_InputField roomCodeInputField;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button closeRoomPanelButton;

        [Header("Data Config")]
        [SerializeField] private GameDataSo gameDataSo;

        public static GameMode SelectedGameMode { get; private set; }
        public static string CurrentRoomCode { get; private set; }
        public static bool IsHost { get; private set; } // YENÝ: Odayý Kurdu mu yoksa Katýlýyor mu?

        private void Awake()
        {
            // Event Dinleyicilerini Baðla
            randomDuelButton.onClick.AddListener(OnRandomDuelClicked);
            friendRoomButton.onClick.AddListener(OnFriendRoomClicked);

            if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
            if (closeRoomPanelButton != null) closeRoomPanelButton.onClick.AddListener(CloseRoomPanel);
        }

        private void Start()
        {
            if (roomPanel != null) roomPanel.SetActive(false);
        }

        #region Button Actions

        /// <summary>
        /// 1v1 Canlý Düello: Anýnda rastgele eþleþme havuzuna girer.
        /// </summary>
        private void OnRandomDuelClicked()
        {
            SelectedGameMode = GameMode.RandomDuel;
            CurrentRoomCode = string.Empty;
            IsHost = true; // Düelloda oda kuran sayýlýr

         

            // Oyun sahnesine geç
            LoadGameScene();
        }

        /// <summary>
        /// Arkadaþýnla Oyna: Oda Kur / Koda Gir panelini açar.
        /// </summary>
        private void OnFriendRoomClicked()
        {
            SelectedGameMode = GameMode.FriendRoom;

            if (roomPanel != null)
            {
                roomPanel.SetActive(true);
            }
        }

        private void OnCreateRoomClicked()
        {
            IsHost = true; // Odayý Kuran Kiþi

            // Rastgele 4 haneli Oda Kodu üret (1000 - 9999)
            CurrentRoomCode = Random.Range(1000, 10000).ToString();
            Debug.Log($"Oda Oluþturuldu. Kod: {CurrentRoomCode}");

            LoadGameScene();
        }

        private const string BaseUrl = "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/";

        private void OnJoinRoomClicked()
        {
            string enteredCode = roomCodeInputField.text.Trim();

            if (string.IsNullOrEmpty(enteredCode))
            {
                Debug.LogWarning("Lütfen geçerli bir oda kodu girin!");
                return;
            }

            StartCoroutine(CheckRoomAndLoadScene(enteredCode));
        }

        private IEnumerator CheckRoomAndLoadScene(string code)
        {
            string checkUrl = $"{BaseUrl}rooms/room_private_{code}.json";

            using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(checkUrl))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success ||
                    string.IsNullOrEmpty(req.downloadHandler.text) ||
                    req.downloadHandler.text == "null")
                {
                    Debug.LogError($"[MENÜ HATA] {code} kodlu oda bulunamadý!");
                    // Buraya kullanýcýya ekranda "Oda bulunamadý" gösteren bir Text/Panel uyarýsý ekleyebilirsin.
                    yield break;
                }
            }

            // Oda varsa bilgileri set et ve oyun sahnesine geç
            IsHost = false;
            CurrentRoomCode = code;
            Debug.Log($"Odaya Katýlýnýyor: {CurrentRoomCode}");

            LoadGameScene();
        }

        private void CloseRoomPanel()
        {
            if (roomPanel != null) roomPanel.SetActive(false);
        }

        private void LoadGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }

        #endregion

        private void OnDestroy()
        {
            // Bellek sýzýntýlarýný önlemek için dinleyicileri temizle
            randomDuelButton.onClick.RemoveAllListeners();
            friendRoomButton.onClick.RemoveAllListeners();
            if (createRoomButton) createRoomButton.onClick.RemoveAllListeners();
            if (joinRoomButton) joinRoomButton.onClick.RemoveAllListeners();
            if (closeRoomPanelButton) closeRoomPanelButton.onClick.RemoveAllListeners();
        }
    }
}
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;
//using System.Collections;

//namespace Passaparola.MainMenu
//{
//    public enum GameMode
//    {
//        RandomDuel,  // 1v1 Canlý Düello
//        FriendRoom   // Arkadaþýnla Oyna
//    }

//    public class MainMenuUIController : MonoBehaviour
//    {
//        [Header("Main Mode Buttons")]
//        [SerializeField] private Button randomDuelButton;
//        [SerializeField] private Button friendRoomButton;

//        [Header("Room Panel Components (Arkadaþýnla Oyna Ýçin)")]
//        [SerializeField] private GameObject roomPanel;
//        [SerializeField] private TMP_InputField roomCodeInputField;
//        [SerializeField] private Button createRoomButton;
//        [SerializeField] private Button joinRoomButton;
//        [SerializeField] private Button closeRoomPanelButton;

//        [Header("Data Config")]
//        [SerializeField] private GameDataSo gameDataSo;

//        public static GameMode SelectedGameMode { get; private set; }
//        public static string CurrentRoomCode { get; private set; }
//        public static bool IsHost { get; private set; } // YENÝ: Odayý Kurdu mu yoksa Katýlýyor mu?

//        private void Awake()
//        {
//            // Event Dinleyicilerini Baðla
//            randomDuelButton.onClick.AddListener(OnRandomDuelClicked);
//            friendRoomButton.onClick.AddListener(OnFriendRoomClicked);

//            if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
//            if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
//            if (closeRoomPanelButton != null) closeRoomPanelButton.onClick.AddListener(CloseRoomPanel);
//        }

//        private void Start()
//        {
//            if (roomPanel != null) roomPanel.SetActive(false);
//        }

//        #region Button Actions

//        /// <summary>
//        /// 1v1 Canlý Düello: Anýnda rastgele eþleþme havuzuna girer.
//        /// </summary>
//        private void OnRandomDuelClicked()
//        {
//            SelectedGameMode = GameMode.RandomDuel;
//            CurrentRoomCode = string.Empty;
//            IsHost = true; // Düelloda oda kuran sayýlýr



//            // Oyun sahnesine geç
//            LoadGameScene();
//        }

//        /// <summary>
//        /// Arkadaþýnla Oyna: Oda Kur / Koda Gir panelini açar.
//        /// </summary>
//        private void OnFriendRoomClicked()
//        {
//            SelectedGameMode = GameMode.FriendRoom;

//            if (roomPanel != null)
//            {
//                roomPanel.SetActive(true);
//            }
//        }

//        private void OnCreateRoomClicked()
//        {
//            IsHost = true; // Odayý Kuran Kiþi

//            // Rastgele 4 haneli Oda Kodu üret (1000 - 9999)
//            CurrentRoomCode = Random.Range(1000, 10000).ToString();
//            Debug.Log($"Oda Oluþturuldu. Kod: {CurrentRoomCode}");

//            LoadGameScene();
//        }

//        private const string BaseUrl = "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/";

//        private void OnJoinRoomClicked()
//        {
//            string enteredCode = roomCodeInputField.text.Trim();

//            if (string.IsNullOrEmpty(enteredCode))
//            {
//                Debug.LogWarning("Lütfen geçerli bir oda kodu girin!");
//                return;
//            }

//            StartCoroutine(CheckRoomAndLoadScene(enteredCode));
//        }

//        private IEnumerator CheckRoomAndLoadScene(string code)
//        {
//            string checkUrl = $"{BaseUrl}rooms/room_private_{code}.json";

//            using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(checkUrl))
//            {
//                yield return req.SendWebRequest();

//                if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success ||
//                    string.IsNullOrEmpty(req.downloadHandler.text) ||
//                    req.downloadHandler.text == "null")
//                {
//                    Debug.LogError($"[MENÜ HATA] {code} kodlu oda bulunamadý!");
//                    // Buraya kullanýcýya ekranda "Oda bulunamadý" gösteren bir Text/Panel uyarýsý ekleyebilirsin.
//                    yield break;
//                }
//            }

//            // Oda varsa bilgileri set et ve oyun sahnesine geç
//            IsHost = false;
//            CurrentRoomCode = code;
//            Debug.Log($"Odaya Katýlýnýyor: {CurrentRoomCode}");

//            LoadGameScene();
//        }

//        private void CloseRoomPanel()
//        {
//            if (roomPanel != null) roomPanel.SetActive(false);
//        }

//        private void LoadGameScene()
//        {
//            SceneManager.LoadScene("GameScene");
//        }

//        #endregion

//        private void OnDestroy()
//        {
//            // Bellek sýzýntýlarýný önlemek için dinleyicileri temizle
//            randomDuelButton.onClick.RemoveAllListeners();
//            friendRoomButton.onClick.RemoveAllListeners();
//            if (createRoomButton) createRoomButton.onClick.RemoveAllListeners();
//            if (joinRoomButton) joinRoomButton.onClick.RemoveAllListeners();
//            if (closeRoomPanelButton) closeRoomPanelButton.onClick.RemoveAllListeners();
//        }
//    }
//}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using MiniJSON; // Firebase JSON verisini okumak için eklendi

namespace Passaparola.MainMenu
{
    public enum GameMode
    {
        RandomDuel,  // 1v1 Canlý Düello
        FriendRoom   // Arkadaþýnla Oyna
    }

    public class MainMenuUIController : MonoBehaviour
    {
        private const string BaseUrl = "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/";

        [Header("Main Mode Buttons")]
        [SerializeField] private Button randomDuelButton;
        [SerializeField] private Button friendRoomButton;

        [Header("Room Panel Components (Arkadaþýnla Oyna Ýçin)")]
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TMP_InputField roomCodeInputField;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button closeRoomPanelButton;

        [Header("UI Feedback (Hata / Bilgi Mesajý)")]
        [SerializeField] private TextMeshProUGUI infoText; // Inspector'dan menüdeki uyarý yazýsýný baðlayýn!

        [Header("Data Config")]
        [SerializeField] private GameDataSo gameDataSo;

        public static GameMode SelectedGameMode { get; private set; }
        public static string CurrentRoomCode { get; private set; }
        public static bool IsHost { get; private set; }

        private void Awake()
        {
            // Event Dinleyicilerini Baðla
            if (randomDuelButton) randomDuelButton.onClick.AddListener(OnRandomDuelClicked);
            if (friendRoomButton) friendRoomButton.onClick.AddListener(OnFriendRoomClicked);

            if (createRoomButton) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            if (joinRoomButton) joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
            if (closeRoomPanelButton) closeRoomPanelButton.onClick.AddListener(CloseRoomPanel);
        }

        private void Start()
        {
            if (roomPanel != null) roomPanel.SetActive(false);
           
        }

        #region Button Actions

        private void OnRandomDuelClicked()
        {
            SelectedGameMode = GameMode.RandomDuel;
            CurrentRoomCode = string.Empty;
            IsHost = true;

            LoadGameScene();
        }

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
            IsHost = true;
            CurrentRoomCode = Random.Range(1000, 10000).ToString();
            Debug.Log($"Oda Oluþturuldu. Kod: {CurrentRoomCode}");

            LoadGameScene();
        }

        private void OnJoinRoomClicked()
        {
            string enteredCode = roomCodeInputField.text.Trim();

            if (string.IsNullOrEmpty(enteredCode))
            {
                ShowInfoMessage("Lütfen geçerli bir oda kodu girin!", Color.yellow);
                return;
            }

            // Katýl butonuna basýlýnca tekrar basýlmasýn diye pasife çekiyoruz
            joinRoomButton.interactable = false;
            ShowInfoMessage("Oda kontrol ediliyor...", Color.white);

            StartCoroutine(CheckRoomAndLoadScene(enteredCode));
        }

        private IEnumerator CheckRoomAndLoadScene(string code)
        {
            string checkUrl = $"{BaseUrl}rooms/room_private_{code}.json";

            using (UnityWebRequest req = UnityWebRequest.Get(checkUrl))
            {
                yield return req.SendWebRequest();

                // 1. KONTROL: Oda Veritabanýnda Var mý?
                if (req.result != UnityWebRequest.Result.Success || string.IsNullOrEmpty(req.downloadHandler.text) || req.downloadHandler.text == "null")
                {
                    ShowInfoMessage("Oda bulunamadý! Kodu kontrol edin.", Color.red);
                    joinRoomButton.interactable = true;
                    yield break; // SAHNEYE GEÇÝÞÝ ENGELLE!
                }

                // 2. KONTROL: Oda Dolu mu? (2 veya daha fazla oyuncu var mý?)
                var roomData = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
                if (roomData != null && roomData.ContainsKey("players"))
                {
                    var players = roomData["players"] as Dictionary<string, object>;
                    if (players != null && players.Count >= 2)
                    {
                        ShowInfoMessage("Bu oda zaten dolu! (Max 2 kiþi)", Color.red);
                        joinRoomButton.interactable = true;
                        yield break; // SAHNEYE GEÇÝÞÝ ENGELLE!
                    }
                }
            }

            // Tüm kontrollerden geçti: Odaya Katýlabilir!
            IsHost = false;
            CurrentRoomCode = code;
            Debug.Log($"Odaya Baþarýyla Katýlýnýyor: {CurrentRoomCode}");

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

        #region Helper UI Methods

        private void ShowInfoMessage(string message, Color color)
        {
            infoPanel.SetActive(true);

            if (infoText != null)
            {
                infoText.text = string.Empty;
                infoText.text = message;
                infoText.color = color;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (randomDuelButton) randomDuelButton.onClick.RemoveAllListeners();
            if (friendRoomButton) friendRoomButton.onClick.RemoveAllListeners();
            if (createRoomButton) createRoomButton.onClick.RemoveAllListeners();
            if (joinRoomButton) joinRoomButton.onClick.RemoveAllListeners();
            if (closeRoomPanelButton) closeRoomPanelButton.onClick.RemoveAllListeners();
        }
    }
}
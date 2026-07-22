using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Passaparola.MainMenu
{
    public class NameEntryPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button confirmButton;

        [Header("Data Config")]
        [SerializeField] private PlayerDataSo player1Data;

        private void Start()
        {
            // Daha önce girilmiþ bir isim varsa InputField'a doldur
            if (player1Data != null && !string.IsNullOrEmpty(player1Data.playerName))
            {
                nameInputField.text = player1Data.playerName;
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(SaveAndClose);
            }

            if (nameInputField != null)
            {
                nameInputField.onEndEdit.AddListener(OnNameEndEdit);
            }
        }

        private void OnNameEndEdit(string enteredName)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SaveAndClose();
            }
        }

        public void SaveAndClose()
        {
            string enteredName = nameInputField.text.Trim();

            if (string.IsNullOrWhiteSpace(enteredName))
            {
                Debug.LogWarning("Lütfen geçerli bir isim girin!");
                return;
            }

            // Scriptable Object'e kaydet
            if (player1Data != null)
            {
                player1Data.playerName = enteredName;
            }

            // Ýsteðe baðlý PlayerPrefs kaydý
            PlayerPrefs.SetString("PlayerName", enteredName);
            PlayerPrefs.Save();

            Debug.Log($"Oyuncu Ýsmi Kaydedildi: {enteredName}");

            // Ýsim panelini kapat
            gameObject.SetActive(false);
        }
    }
}
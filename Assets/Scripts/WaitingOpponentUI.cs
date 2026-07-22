using UnityEngine;
using TMPro;
using Passaparola.MainMenu;

public class WaitingOpponentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI waitingText;

    private RectTransform _waitingTextRect;
    private Vector2 _originalWaitingTextPosition;
    private bool _isPositionSaved = false;

    private void Awake()
    {
        SaveOriginalPosition();
    }

    private void SaveOriginalPosition()
    {
        if (waitingText != null && !_isPositionSaved)
        {
            _waitingTextRect = waitingText.GetComponent<RectTransform>();
            if (_waitingTextRect != null)
            {
                // Editor'de ayarladýðýn orijinal (alt) pozisyonu hafýzaya alýyoruz
                _originalWaitingTextPosition = _waitingTextRect.anchoredPosition;
                _isPositionSaved = true;
            }
        }
    }

    private void OnEnable()
    {
        UpdateRoomCodeUI();
    }

    private void UpdateRoomCodeUI()
    {
        if (waitingText == null) return;

        // OnEnable Awake'den önce tetiklenme ihtimaline karþý garantiye alalým
        SaveOriginalPosition();

        string code = MainMenuUIController.CurrentRoomCode;

        if (!string.IsNullOrEmpty(code))
        {
            // --- ÖZEL ODA (ARKADAÞLA OYNA) ---
            if (roomCodeText != null)
            {
                roomCodeText.gameObject.SetActive(true);
                roomCodeText.text = $"ODA KODU: {code}";
            }

            waitingText.text = "RAKÝP BEKLENÝYOR...";

            // Orijinal alt pozisyonuna geri getir
            if (_waitingTextRect != null)
            {
                _waitingTextRect.anchoredPosition = _originalWaitingTextPosition;
            }
        }
        else
        {
            // --- RASTGELE DÜELLO ---
            if (roomCodeText != null)
            {
                roomCodeText.gameObject.SetActive(false); // Kod yazýsýný tamamen gizle
            }

            waitingText.text = "RAKÝP ARANIYOR...";

            // Panelin dikey ortasýna çek (Y = 0 yapýyoruz, X pozisyonunu koruyoruz)
            if (_waitingTextRect != null)
            {
                _waitingTextRect.anchoredPosition = new Vector2(_originalWaitingTextPosition.x, 0f);
            }
        }
    }
}
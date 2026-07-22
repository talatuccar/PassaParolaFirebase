using UnityEngine;
using Passaparola.Multiplayer;
using System.Collections;

public class DualBoardController : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private LetterBoardView localBoardView;   // Oyuncunun kendi çemberi
    [SerializeField] private LetterBoardView remoteBoardView;  // Rakibin küçük çemberi

    [Header("Layout Configurations")]
    [SerializeField] private GameDataSo gameDataSo;

    private void Awake()
    {
        SetupBoards();
    }

    public void SetupBoards()
    {
        // Kendi çemberini oluþtur (Büyük)
        localBoardView.InitializeBoard(gameDataSo.DefaultLayoutRadius, gameDataSo.DefaultLayoutSize);

        // Rakibin çemberini oluþtur (Küçük - Alt Kýsýmda)
        remoteBoardView.InitializeBoard(gameDataSo.firstPlayerResultLayoutRadius, gameDataSo.firstPlayerResultLayoutSize);
    }

    /// <summary>
    /// Kendi verdiðimiz cevabý anýnda kendi tahtamýzda günceller.
    /// </summary>
    public void UpdateLocalAnswer(int questionIndex, AnswerStatus status)
    {
        localBoardView.SetLetterStatus(questionIndex, status);
    }

    /// <summary>
    /// Firebase'den gelen rakip cevabýný anýnda rakip tahtasýnda günceller.
    /// </summary>
    public void UpdateRemoteAnswer(int questionIndex, AnswerStatus status)
    {
        remoteBoardView.SetLetterStatus(questionIndex, status);
    }


    /// <summary>
    /// Aktif olan sorunun harfini (TextMeshProUGUI) yanýp söndürür.
    /// </summary>
    /// <summary>
    /// Aktif olan sorunun harfini (TextMeshProUGUI) yanýp söndürür.
    /// </summary>
    public IEnumerator BlinkLocalLetter(int questionIndex)
    {
        // 1. Büyük çemberdeki ilgili harfin GameObject'ini al
        GameObject localItem = localBoardView.GetLetterObject(questionIndex);
        if (localItem == null) yield break;

        // 2. Harf üzerindeki TextMeshProUGUI'yý bul
        TMPro.TextMeshProUGUI letterText = localItem.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (letterText == null) yield break;

        yield return new WaitForSeconds(0.1f);

        // 3. Blink döngüsü
        while (true)
        {
            letterText.enabled = false;
            yield return new WaitForSeconds(0.4f);
            letterText.enabled = true;
            yield return new WaitForSeconds(0.4f);
        }
    }

    /// <summary>
    /// Harf yanýp sönerken yarýda kesilirse yazýnýn kapalý kalmasýný önler.
    /// </summary>
    public void ResetLocalLetterVisibility(int questionIndex)
    {
        GameObject localItem = localBoardView.GetLetterObject(questionIndex);
        if (localItem == null) return;

        TMPro.TextMeshProUGUI letterText = localItem.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (letterText != null)
        {
            letterText.enabled = true;
        }
    }
}

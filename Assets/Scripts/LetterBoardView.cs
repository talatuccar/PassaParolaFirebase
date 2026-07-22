using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Passaparola.Multiplayer;

public class LetterBoardView : MonoBehaviour
{
    [SerializeField] private GameObject letterPrefab;
    [SerializeField] private int totalCount = 21;

    private readonly List<GameObject> _spawnedLetters = new List<GameObject>();

    /// <summary>
    /// Verilen yarýçap ve ölçekte harf çemberini oluþturur.
    /// </summary>
    public void InitializeBoard(int radius, float scale)
    {
        ClearBoard();

        if (letterPrefab == null)
        {
            Debug.LogError($"[LetterBoardView] '{gameObject.name}' objesindeki letterPrefab Inspector'da BAÐLI DEÐÝL!");
            return;
        }

        // Scale 0 veya negatif gelirse varsayýlan 1f yap
        if (scale <= 0) scale = 1f;

        for (int i = 0; i < totalCount; i++)
        {
            float angle = -i * Mathf.PI * 2f / totalCount - Mathf.PI / 2f;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            GameObject letterObj = Instantiate(letterPrefab, transform);

            RectTransform rectTransform = letterObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = pos;
                // Z pozisyonunu sýfýrlayarak Canvas arkasýnda kalmasýný engelliyoruz:
                rectTransform.localPosition = new Vector3(pos.x, pos.y, 0f);
            }

            letterObj.transform.localScale = new Vector3(scale, scale, 1f);

            var textComponent = letterObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = LetterData.GetLetter(i);
            }

            _spawnedLetters.Add(letterObj);
        }
    }

    /// <summary>
    /// Belirtilen indeksteki harfin rengini günceller.
    /// </summary>
    public void SetLetterStatus(int index, AnswerStatus status)
    {
        if (index < 0 || index >= _spawnedLetters.Count) return;

        GameObject letterObj = _spawnedLetters[index];
        Image img = letterObj.GetComponent<Image>();
        TextMeshProUGUI text = letterObj.GetComponentInChildren<TextMeshProUGUI>();

        if (img == null) return;

        if (text != null)
        {
            text.enabled = true;
        }

        switch (status)
        {
            case AnswerStatus.Correct:
                img.color = Color.green;
                if (text != null) text.color = Color.white; // Yeþil üzerine beyaz harf
                break;

            case AnswerStatus.Wrong:
                img.color = Color.red; // Çember kýpkýrmýzý kalýr
                if (text != null) text.color = Color.white; // Ama harf BEYAZ olur, cam gibi okunur!
                break;

            case AnswerStatus.Passed:
                img.color = Color.yellow;
                if (text != null) text.color = Color.black; // Sarý üzerine siyah harf
                break;

            default:
                img.color = Color.white;
                if (text != null) text.color = Color.black;
                break;
        }
    }
    private void ClearBoard()
    {
        foreach (var letter in _spawnedLetters)
        {
            if (letter != null)
            {
                Destroy(letter);
            }
        }
        _spawnedLetters.Clear();
    }

    public GameObject GetLetterObject(int index)
    {
        if (index >= 0 && index < _spawnedLetters.Count)
        {
            return _spawnedLetters[index];
        }
        return null;
    }
}
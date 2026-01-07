
using TMPro;
using UnityEngine;

public class GameOverPanelUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI correctText;
    [SerializeField] TextMeshProUGUI wrongText;
    [SerializeField] TextMeshProUGUI passedText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI nameText;
    public void SetValues(int correct, int wrong, int passed, int score,string name)
    {
        correctText.text = "Correct Count: " + correct;
        wrongText.text = "Wrong Count: " + wrong;
        passedText.text = "Passed Count: " + passed;
        scoreText.text = "SCORE: " + score;
        nameText.text = name;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public class CreateYourData : MonoBehaviour
{

    
    HashSet<TMP_InputField> enteredQuestions = new HashSet<TMP_InputField>();
    HashSet<TMP_InputField> enteredAnswers = new HashSet<TMP_InputField>();
    private int answerCount;
    private int questionCount;
    private int maxAsk;

    public DynamicDataSo dynamicDataSo;
    public AlertButton alertButton;
    [SerializeField] GameObject parentWithInputFields;
    [SerializeField] GameObject playerDataPanel;
    

    [SerializeField] GameObject alertPanel;
    [SerializeField] TextMeshProUGUI alertText;

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button openButton;
    [SerializeField] Button saveButton;
    
    List<TMP_InputField> questionFields = new List<TMP_InputField>();
    List<TMP_InputField> answerFields = new List<TMP_InputField>();
    Dictionary<string, string> dataDict = new Dictionary<string, string>();
    void Start()
    {
        maxAsk = LetterData.Count;

        dataDict.Clear();

        saveButton.onClick.AddListener(SavedQuestionData);
        startButton.onClick.AddListener(IsReadyForGame);
        openButton.onClick.AddListener(OpenPanel);
    }
    void OpenPanel()
    {
        playerDataPanel.SetActive(true);
        SetPlaceholders();
    }
    void SetPlaceholders()
    {
        int childCount = parentWithInputFields.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            string letter = GetLetter(i);

            Transform child = parentWithInputFields.transform.GetChild(i);

            TMP_InputField[] inputs = child.GetComponentsInChildren<TMP_InputField>();

            if (inputs.Length >= 2)
            {
                TMP_InputField question = inputs[0];
                TMP_InputField answer = inputs[1];
                question.onSubmit.AddListener((string text) => CheckQuestions(text, letter, question));
                answer.onSubmit.AddListener((string input) => CheckAnswer(input, letter, answer));

                questionFields.Add(question);
                answerFields.Add(answer);

                if (question.placeholder is TextMeshProUGUI qPlaceholder)
                {
                    qPlaceholder.text = letter + " harfinin sorusu";
                }

                if (answer.placeholder is TextMeshProUGUI aPlaceholder)
                {
                    aPlaceholder.text = letter;
                }
            }
        }
    }

    void CheckAnswer(string input, string correctLetter, TMP_InputField answerInputField)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (input[0].ToString().ToUpper() != correctLetter.ToUpper())
            {
                SetInputFieldBorderColor(answerInputField, Color.red);
                string message = "Cevap " + correctLetter + " harfi ile baþlamalýdýr.";
                alertButton.AlertPanelStatus(message);
                alertButton.SetAnswerFieldsInteractable(false, answerFields);
            }
            else
            {
                SetInputFieldBorderColor(answerInputField, Color.gray);

                if (!enteredAnswers.Contains(answerInputField))
                {
                    enteredAnswers.Add(answerInputField);
                    answerCount++;
                    dynamicDataSo.playerAnswers.Add(input);
                }
                else
                {
                    int index = answerFields.IndexOf(answerInputField);
                    dynamicDataSo.playerAnswers[index] = input;
                }

                print(answerCount);
            }
        }
        else
        {
            string message = correctLetter + " ÞIKKI ÝÇÝN CEVAP METNÝ BOÞ!";
            alertButton.AlertPanelStatus(message);
            alertButton.SetAnswerFieldsInteractable(false, answerFields);
        }
    }
    void CheckQuestions(string text, string correctLetter, TMP_InputField questionInputField)
    {
        if (!string.IsNullOrEmpty(text))
        {
            SetInputFieldBorderColor(questionInputField, Color.gray);

            if (!enteredQuestions.Contains(questionInputField))
            {
                enteredQuestions.Add(questionInputField);
                questionCount++;
                dynamicDataSo.playerQuestions.Add(text);
            }
            else
            {
                int index = questionFields.IndexOf(questionInputField);
                dynamicDataSo.playerQuestions[index] = text;
            }

            print(questionCount);
        }
        else
        {
            string message = correctLetter + " ÞIKKI ÝÇÝN SORU METNÝ BOÞ!";
            alertButton.AlertPanelStatus(message);
            alertButton.SetAnswerFieldsInteractable(false, questionFields);
        }
    }

    void SetInputFieldBorderColor(TMP_InputField inputField, Color color)
    {

        Image inputFieldImage = inputField.GetComponent<Image>();
        if (inputFieldImage != null)
        {
            inputFieldImage.color = color;
        }
    }
    string GetLetter(int index)
    {

        if (index >= 0 && index < LetterData.Count)
        {
            return LetterData.GetLetter(index);
        }
        else
        {
            Debug.LogWarning("Geçersiz index: " + index);
            return null;
        }
    }
    public void IsReadyForGame()
    {
        if (questionCount == maxAsk && answerCount == maxAsk)
        {
            dynamicDataSo.isPlayerData = true;
        }
        else
        {
            string message = "TÜM METÝNLER DOLU OLMALI!";
            alertButton.AlertPanelStatus(message);
            alertButton.SetAnswerFieldsInteractable(false, answerFields);
            return;
        }

        SceneManager.LoadScene("GameScene");
    }
    public void SavedQuestionData()
    {
        dataDict.Clear();
        if (questionCount == maxAsk && answerCount == maxAsk)
        {
            for (int i = 0; i < dynamicDataSo.playerQuestions.Count; i++)
            {
                string key = dynamicDataSo.playerQuestions[i];
                string value = dynamicDataSo.playerAnswers[i];

                if (!dataDict.ContainsKey(key))
                    dataDict[key] = value;

            }

            SaveManager.SaveDictionary(dataDict);
        }
        else
        {
            string message = "TÜM METÝNLER DOLU OLMALI!";
            alertButton.AlertPanelStatus(message);
            alertButton.SetAnswerFieldsInteractable(false, answerFields);
        }
    }
    public void OpenSavedAsks()
    {
        StartCoroutine(OpenSavedAsksWithTime());
    }

    IEnumerator OpenSavedAsksWithTime()
    {
        if (SaveManager.HasKey())
        {     
            dynamicDataSo.isPlayerData = true;
            SavedSettings(); 
            print(SaveManager.HasKey());
        }
        else
        {
            string message = "KAYDEDÝLEN VERÝ YOK";
            alertButton.AlertPanelStatus(message);
            yield break;

        }
    }
    void SavedSettings()
    {
        dataDict.Clear();
        dynamicDataSo.playerQuestions.Clear();
        dynamicDataSo.playerAnswers.Clear();

        dataDict = SaveManager.LoadDictionary();

        foreach (var item in dataDict)
        {
            dynamicDataSo.playerQuestions.Add(item.Key);
            dynamicDataSo.playerAnswers.Add(item.Value);
            
        }
        SceneManager.LoadScene("GameScene");
    }
}
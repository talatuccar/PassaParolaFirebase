using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;


public class GameManager : MonoBehaviour
{
    public TMP_InputField answerInputField;

    public CircularLayout circularLayout;

    public static GameManager instance;
    private bool isTimeUpHandled = false;

    bool allQuestionAsked;
    bool startGame;
    const int maxQuestion = 22;
    int passedQuestionsIndex = 0;

    [SerializeField] TextMeshProUGUI questionText;

    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TMP_InputField playerNameField;
    [SerializeField] private GameObject playerNamePanel;


    public UIManager uiManager;

    public static event Action<int> OnNextAsk;

    private List<GameObject> alphabet = new List<GameObject>();
    private List<int> passedQuestions = new List<int>();

    int currentQuestionIndex;

    private TimeManager timeManager;
    private int correctCount = 0;
    private int wrongCount = 0;
    private int passedCount = 0;

    string currentQuestion;
    string currentAnswer;

    [Header("ScriptableObjects")]
    public PlayerDataSo player1Data;
    public PlayerDataSo player2Data;
    public SoundDataSo soundDataSo;
    public GameDataSo gameDataSo;
    public DynamicDataSo dynamicDataSo;

    private bool isPlayerData;
    public bool IsPlayerData
    {
        get { return isPlayerData; }
        set { isPlayerData = value; }
    }
    public static bool isSecondPlayer = false;
    void Awake()
    {
        Screen.fullScreen = true;
        if (instance == null)
            instance = this;
        else if (instance != this)
        {

            Destroy(gameObject);
        }
        isPlayerData = dynamicDataSo.isPlayerData;
    }
    void Start()
    {
        
      
        PlatformSettings();
        playerNameField.onEndEdit.AddListener(OnEndEdit);

        answerInputField.onSubmit.AddListener(OnSubmit);
        StartCoroutine(CountdownAndStartGame());

        timeManager = FindFirstObjectByType<TimeManager>();
        if (timeManager != null)
            timeManager.OnTimeUp += HandleTimeUp;
        else
            Debug.LogError("TimeManager bulunamadý!");

    }

    void PlatformSettings()
    {
        {

            if (Application.isMobilePlatform)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                StartCoroutine(DelayedActivateInput());
            }
            else
            {
                FocusNameInput();
            }
        }
    }
    IEnumerator DelayedActivateInput()
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(playerNameField.gameObject);
        playerNameField.OnPointerClick(new PointerEventData(EventSystem.current));
    }
    void FocusNameInput()
    {
        playerNameField.Select();
        playerNameField.ActivateInputField();
    }
    void FocusAnswerInput()
    {
        answerInputField.Select();
        answerInputField.ActivateInputField();
    }
    void OnEndEdit(string text)
    {
        if (isSecondPlayer)
        {
            player2Data.playerName = text;
            NameEntered();
        }
        else
        {
            player1Data.playerName = text;
            NameEntered();
        }
    }
    void NameEntered()
    {
        playerNamePanel.SetActive(false);
        startGame = true;
    }
    void EndGame()
    {
        passedCount = passedQuestions.Count;
        if (isSecondPlayer)
        {
            player2Data.GetScore(correctCount, wrongCount, passedCount);
            isSecondPlayer = false;
            uiManager.InstantiatePanel();

        }
        else
        {
            player1Data.GetScore(correctCount, wrongCount, passedCount);
            isSecondPlayer = true;
            uiManager.Replay();
        }
    }
    private void HandleTimeUp()
    {
        if (isTimeUpHandled) return;

        isTimeUpHandled = true;

        answerInputField.interactable = false;
        StopAllCoroutines();
        EndGame();
        
    }
    void OnSubmit(string text)
    {
        //if (EventSystem.current.currentSelectedGameObject != inputField.gameObject)
        //{
        //    Debug.Log("Input focus baþka yere kaydý, iþlem yapýlmayacak.");
        //    return;
        //}
        // Kullanýcý metni bitirdiðinde yapýlacak iþlem
        CompareResult(text);
        answerInputField.text = "";
        FocusAnswerInput();
    }
    void IsDone()
    {
        if (currentQuestionIndex < maxQuestion && !allQuestionAsked)
        {
            return;
        }

        if (currentQuestionIndex == maxQuestion && passedQuestions.Count > 0)
        {

            allQuestionAsked = true;
            passedQuestionsIndex = 0;
            currentQuestionIndex = passedQuestions[passedQuestionsIndex];
            return;
        }

        if (passedQuestionsIndex == passedQuestions.Count)
        {
            passedQuestionsIndex = 0;
            currentQuestionIndex = passedQuestions[passedQuestionsIndex];
            return;

        }

        if (passedQuestionsIndex < passedQuestions.Count)
        {
            currentQuestionIndex = passedQuestions[passedQuestionsIndex];
            return;
        }
    }
    void CompareResult(string userAnswer)
    {
        if (currentAnswer.Trim().ToLower() == userAnswer.Trim().ToLower())
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
        NextQuestion();
    }

    void NextQuestion()
    {
        StopAllCoroutines();
        EnabledText();
        currentQuestionIndex++;
        print(currentQuestionIndex);
        IsDone();
        Ask(isPlayerData);
    }
    void EnabledText()
    {
        TextMeshProUGUI text = alphabet[currentQuestionIndex].GetComponentInChildren<TextMeshProUGUI>();
        if (!text.enabled)
        {
            text.enabled = true;
        }
    }
    void CorrectAnswer()
    {
        SoundController.Instance.PlayChoiseVoice(soundDataSo.correctClip);

        alphabet[currentQuestionIndex].GetComponent<Image>().color = Color.green;
        if (passedQuestions.Contains(currentQuestionIndex))
            passedQuestions.Remove(currentQuestionIndex);

        correctCount++;
        gameDataSo.CorrectList(currentQuestionIndex);
    }
    void WrongAnswer()
    {
        SoundController.Instance.PlayChoiseVoice(soundDataSo.wrongClip);
        alphabet[currentQuestionIndex].GetComponent<Image>().color = Color.red;
        if (passedQuestions.Contains(currentQuestionIndex))
            passedQuestions.Remove(currentQuestionIndex);

        wrongCount++;
        gameDataSo.WrongList(currentQuestionIndex);

    }
    public void Passed()
    {
        SoundController.Instance.PlayChoiseVoice(soundDataSo.passedClip);
        passedQuestionsIndex++;

        if (!passedQuestions.Contains(currentQuestionIndex))
        {
            passedQuestions.Add(currentQuestionIndex);
            alphabet[currentQuestionIndex].GetComponent<Image>().color = Color.yellow;

            gameDataSo.PassedList(currentQuestionIndex);
        }
        NextQuestion();
        FocusAnswerInput();
    }
    public void GetQuestionData(string question, string answer)
    {
        currentQuestion = question;
        currentAnswer = answer;
    }
    public void GetList(List<GameObject> getAlphabet)
    {
        alphabet = getAlphabet;
    }
    void Ask(bool isPlayer)
    {
        if (isPlayer)
        {
            GetQuestionData(dynamicDataSo.GetQuestion(currentQuestionIndex), dynamicDataSo.GetAnswer(currentQuestionIndex));
            PrepareForNext();
            return;

        }
        OnNextAsk?.Invoke(currentQuestionIndex);
        PrepareForNext();
        //char firstLetter = currentAnswer.ToUpper()[0];
    }
    void PrepareForNext()
    {
        questionText.text = currentQuestion;
        TextMeshProUGUI text = alphabet[currentQuestionIndex].GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(Blink(text));
    }
    IEnumerator Blink(TextMeshProUGUI currentLetter)
    {
        yield return new WaitForSeconds(0.1f);

        while (true)
        {
            currentLetter.enabled = false;
            yield return new WaitForSeconds(0.4f);
            currentLetter.enabled = true;
            yield return new WaitForSeconds(0.4f);
        }
    }
    IEnumerator CountdownAndStartGame()
    {
        yield return new WaitUntil(() => startGame);
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        timeManager.Started = true;
        circularLayout.MakeCircularLayout(gameDataSo.DefaultLayoutRadius, gameDataSo.DefaultLayoutSize);
        if (isSecondPlayer)
            circularLayout.MakeCircularLayout(gameDataSo.firstPlayerResultLayoutRadius, gameDataSo.firstPlayerResultLayoutSize, gameDataSo);

        Ask(isPlayerData);
        FocusAnswerInput();
    }
    public bool IsGameActive()
    {
        return timeManager != null && timeManager.Started && startGame && !isTimeUpHandled;
    }
}
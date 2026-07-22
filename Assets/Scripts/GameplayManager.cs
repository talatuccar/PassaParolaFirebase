using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Passaparola.Multiplayer;
using Passaparola.MainMenu;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Controllers & Managers")]
    [SerializeField] private DualBoardController boardController;
    [SerializeField] private FirebaseNetworkAdapter networkAdapter;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private QuestionLoader questionLoader;

    [Header("UI References")]
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Panels")]
    [SerializeField] private GameObject waitingOpponentPanel;  // Rakip Bekleme Paneli

    [Header("Scriptable Objects (Data)")]
    public PlayerDataSo player1Data; // Local Oyuncu (Sen)
    public PlayerDataSo player2Data; // Remote Oyuncu (Rakip)
    public SoundDataSo soundDataSo;
    public GameDataSo gameDataSo;
    public DynamicDataSo dynamicDataSo;

    // Soru ve Oyun Durum Deðiþkenleri
    private int _currentQuestionIndex = 0;
    private const int MaxQuestions = 21;

    private readonly List<int> _passedQuestions = new List<int>();
    private int _passedQuestionsIndex = 0;
    private bool _allQuestionsAsked = false;

    private int _correctCount = 0;
    private int _wrongCount = 0;
    private int _passedCount = 0;

    private string _currentQuestion;
    private string _currentAnswer;
    private Coroutine _blinkCoroutine;
    private bool _isTimeUpHandled = false;
    public bool _isGameActive = false;

    public static event Action<int> OnNextAsk;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
      



        answerInputField.onSubmit.AddListener(OnAnswerSubmitted);

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // Katýlan (Join) tarafsa bekleme panelini açma, kuran (Host) tarafsa göster!
        if (waitingOpponentPanel != null)
        {
            waitingOpponentPanel.SetActive(MainMenuUIController.IsHost);
        }

        // Að dinleyicisini baðla
        networkAdapter.ListenToOpponentAnswers(OnOpponentAnswerReceived);

        if (timeManager != null)
        {
            timeManager.OnTimeUp += HandleTimeUp;
        }

        // Að baðlantýsýný ve oda eþleþmesini baþlat
        networkAdapter.ConnectAndFindMatch(OnRoomReady);
    }

    /// <summary>
    /// Rakip/Arkadaþ odaya katýldýðýnda (Oda 2 kiþi olduðunda) tetiklenir.
    /// </summary>
    private void OnRoomReady(string roomId)
    {
        Debug.Log($"Odaya Ýki Oyuncu Katýldý. Oda ID: {roomId}");

        // Her iki oyuncuda da bekleme panelini kapat
        if (waitingOpponentPanel != null) waitingOpponentPanel.SetActive(false);

        StartCoroutine(CountdownAndStartGame());
    }

    private IEnumerator CountdownAndStartGame()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.gameObject.SetActive(false);
        }

        _isGameActive = true;
        if (timeManager != null) timeManager.Started = true;

        AskQuestion();
        FocusAnswerInput();
    }

    #region 2. Soru Cevaplama ve Pas Mantýðý

    private void OnAnswerSubmitted(string userAnswer)
    {
        if (!_isGameActive || string.IsNullOrWhiteSpace(userAnswer)) return;

        CompareResult(userAnswer);
        answerInputField.text = "";
        FocusAnswerInput();
    }

    private void CompareResult(string userAnswer)
    {
        bool isCorrect = string.Equals(_currentAnswer.Trim(), userAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }

        NextQuestion();
    }

    private void CorrectAnswer()
    {
        if (SoundController.Instance != null && soundDataSo != null)
            SoundController.Instance.PlayChoiseVoice(soundDataSo.correctClip);

        if (_passedQuestions.Contains(_currentQuestionIndex))
            _passedQuestions.Remove(_currentQuestionIndex);

        _correctCount++;
        if (gameDataSo != null) gameDataSo.CorrectList(_currentQuestionIndex);

        boardController.UpdateLocalAnswer(_currentQuestionIndex, AnswerStatus.Correct);
        networkAdapter.SendMyAnswer(_currentQuestionIndex, AnswerStatus.Correct);
    }

    private void WrongAnswer()
    {
        if (SoundController.Instance != null && soundDataSo != null)
            SoundController.Instance.PlayChoiseVoice(soundDataSo.wrongClip);

        if (_passedQuestions.Contains(_currentQuestionIndex))
            _passedQuestions.Remove(_currentQuestionIndex);

        _wrongCount++;
        if (gameDataSo != null) gameDataSo.WrongList(_currentQuestionIndex);

        boardController.UpdateLocalAnswer(_currentQuestionIndex, AnswerStatus.Wrong);
        networkAdapter.SendMyAnswer(_currentQuestionIndex, AnswerStatus.Wrong);
    }

    public void OnPassButtonClicked()
    {
        if (!_isGameActive) return;

        if (SoundController.Instance != null && soundDataSo != null)
            SoundController.Instance.PlayChoiseVoice(soundDataSo.passedClip);

        _passedQuestionsIndex++;

        if (!_passedQuestions.Contains(_currentQuestionIndex))
        {
            _passedQuestions.Add(_currentQuestionIndex);
            if (gameDataSo != null) gameDataSo.PassedList(_currentQuestionIndex);
        }

        boardController.UpdateLocalAnswer(_currentQuestionIndex, AnswerStatus.Passed);
        networkAdapter.SendMyAnswer(_currentQuestionIndex, AnswerStatus.Passed);

        NextQuestion();
        FocusAnswerInput();
    }

    #endregion

    #region 3. Soru Geçiþleri ve Akýþ (IsDone Mantýðý)

    private void NextQuestion()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);

        boardController.ResetLocalLetterVisibility(_currentQuestionIndex);

        _currentQuestionIndex++;

        IsDone();

        if (_isGameActive)
        {
            AskQuestion();
        }
    }

    private void IsDone()
    {
        if (_currentQuestionIndex < MaxQuestions && !_allQuestionsAsked)
        {
            return;
        }

        if (_currentQuestionIndex == MaxQuestions && _passedQuestions.Count > 0)
        {
            _allQuestionsAsked = true;
            _passedQuestionsIndex = 0;
            _currentQuestionIndex = _passedQuestions[_passedQuestionsIndex];
            return;
        }

        if (_passedQuestionsIndex == _passedQuestions.Count && _passedQuestions.Count > 0)
        {
            _passedQuestionsIndex = 0;
            _currentQuestionIndex = _passedQuestions[_passedQuestionsIndex];
            return;
        }

        if (_passedQuestionsIndex < _passedQuestions.Count)
        {
            _currentQuestionIndex = _passedQuestions[_passedQuestionsIndex];
            return;
        }

        if (_allQuestionsAsked)
        {
            EndGame();
        }
    }

    private void AskQuestion()
    {
        
        OnNextAsk?.Invoke(_currentQuestionIndex);

        _blinkCoroutine = StartCoroutine(boardController.BlinkLocalLetter(_currentQuestionIndex));
    }

    public void GetQuestionData(string question, string answer)
    {
        _currentQuestion = question;
        _currentAnswer = answer;

        if (questionText != null)
        {
            questionText.text = _currentQuestion;
        }
    }

    #endregion

    #region 4. Rakip Að Dinleme & Skor Kaydý

    private void OnOpponentAnswerReceived(int questionIndex, AnswerStatus status)
    {
        boardController.UpdateRemoteAnswer(questionIndex, status);
    }

    private void FocusAnswerInput()
    {
        if (answerInputField != null)
        {
            answerInputField.Select();
            answerInputField.ActivateInputField();
        }
    }

    private void HandleTimeUp()
    {
        if (_isTimeUpHandled) return;
        _isTimeUpHandled = true;

        EndGame();
    }

    private void EndGame()
    {
        _isGameActive = false;
        if (timeManager != null) timeManager.Started = false;
        answerInputField.interactable = false;

        _passedCount = _passedQuestions.Count;

        if (player1Data != null)
        {
            player1Data.GetScore(_correctCount, _wrongCount, _passedCount);
        }

        Debug.Log($"Oyun Bitti! {player1Data.playerName} -> Doðru: {_correctCount}, Yanlýþ: {_wrongCount}, Pas: {_passedCount}");
    }

    #endregion
}
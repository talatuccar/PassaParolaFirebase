
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData")]
public class GameDataSo : ScriptableObject
{
  
    public float firstPlayerResultLayoutSize { get; } = 0.65f;
    public int firstPlayerResultLayoutRadius { get; } = -400;
    private int defaultLayoutSize = 1;
    private int defaultLayoutRadius = -480;

    public int DefaultLayoutSize => defaultLayoutSize;
    public int DefaultLayoutRadius => defaultLayoutRadius;

    public Queue<int> correctList = new Queue<int>();
    public Queue<int> wrongList = new Queue<int>();
    public Queue<int> passedList = new Queue<int>();

    public void CorrectList(int correctObject)
    {
        correctList.Enqueue(correctObject);
    }
    public void WrongList(int wrongObject)
    {
        wrongList.Enqueue(wrongObject);
    }
    public void PassedList(int passedObject)
    {
        passedList.Enqueue(passedObject);
    }  
    public void ResetData()
    {
        correctList.Clear();
        wrongList.Clear();
        passedList.Clear();
    }
    /*
    alternative way to manage the question pool;
    public enum QuestionSet
    {
        Questions1, // Ýlk set, yani questions.json
        Questions2  // Ýkinci set, yani questions2.json
    }

    public QuestionSet currentQuestionSet = QuestionSet.Questions1;
    */
}

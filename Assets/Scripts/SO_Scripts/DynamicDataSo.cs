
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DynamicDataSo", menuName = "ScriptableObjects/DynamicDataSo", order = 1)]
public class DynamicDataSo : ScriptableObject
{  
    public bool isPlayerData;
  
    public List<string> playerQuestions= new List<string>();
    public List<string> playerAnswers = new List<string>();

    private void OnEnable()
    { 
        isPlayerData = false;
       
        playerQuestions.Clear();
        playerAnswers.Clear();
    }
    public string GetQuestion(int index)
    {
        if (index >= 0 && index < playerQuestions.Count)
        {
            return playerQuestions[index];
        }
        return null;
    }

    public string GetAnswer(int index)
    {
        if (index >= 0 && index < playerAnswers.Count)
        {
            return playerAnswers[index];
        }
        return null;
    }
}
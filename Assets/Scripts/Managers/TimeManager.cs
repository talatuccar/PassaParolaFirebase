using UnityEngine;
using System;
using TMPro;

public class TimeManager : MonoBehaviour
{
    private float gameDuration = 120f; 
    private float timer;

    [SerializeField] private TextMeshProUGUI timeText;  
    public  event Action OnTimeUp;
    private bool started;
    public bool Started
    {
        get => started;
        set { started = value; }
    }
    void Update()
    {
        if (!started) return;

        timer += Time.deltaTime;

        if (timer >= gameDuration)
        {
           OnTimeUp?.Invoke();
            timeText.text = "00:00";
            timeText.color = Color.red; 
        }
        else
        {
            float remainingTime = gameDuration - timer;
            int seconds = Mathf.FloorToInt(remainingTime);

            if (remainingTime <= 15f)
            {
                timeText.color = Color.red;
            }
            else
            {
                timeText.color = Color.white; 
            }

            timeText.text = string.Format("{0:D2}:{1:D2}", seconds / 60, seconds % 60);
        }
    } 
}

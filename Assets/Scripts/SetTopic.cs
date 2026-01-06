
using UnityEngine;
public class SetTopic : MonoBehaviour
{
    public GameDataSo gameDataSo;
    public GameObject topicPanel;
    public static bool isClose = true;
    public DynamicDataSo dynamicDataSo;

   
    public void SetTopicQuestions()
    {

        gameDataSo.QuestionTopic = TopicEnum.PassaParolaQuestions;
        dynamicDataSo.isPlayerData = false;
    }

  
    public void OpenTopicPanel()
    {
        //if (isClose)
        //{
        //    topicPanel.SetActive(true);
        //}
        //else
        //    topicPanel.SetActive(false);
        topicPanel.SetActive(isClose);
        isClose = !isClose;


        
    }
}



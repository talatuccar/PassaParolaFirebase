using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGame : MonoBehaviour
{
    public GameDataSo gameDataSo;
    public void Play()
    {
        if(gameDataSo.QuestionTopic != TopicEnum.PlayerQuestions)
          gameDataSo.QuestionTopic = TopicEnum.PassaParolaQuestions; // set default topic 
        SceneManager.LoadScene("GameScene");
    }
}

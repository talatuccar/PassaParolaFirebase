using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGame : MonoBehaviour
{
    public GameDataSo gameDataSo;
    public void Play()
    {
        SceneManager.LoadScene("GameScene");
    }
}

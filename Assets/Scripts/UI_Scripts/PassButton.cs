using UnityEngine;
using UnityEngine.UI;
public class PassButton : MonoBehaviour
{
    Button passButton;
    void Start()
    {
        passButton = GetComponent<Button>();
        passButton.onClick.AddListener(PassButtonClicked);
       
    }
    void Update()
    {
        if (GameplayManager.Instance != null && GameplayManager.Instance._isGameActive)
        {
            passButton.interactable = true;
        }
        else
        {
            passButton.interactable = false;
        }
    }
    public void PassButtonClicked()
    {
        if (passButton != null)
        {

            GameplayManager.Instance.OnPassButtonClicked();

        }
    }
}

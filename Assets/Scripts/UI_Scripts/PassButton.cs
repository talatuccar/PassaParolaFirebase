using System.Collections;
using System.Collections.Generic;
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

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.IsGameActive())
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

            GameManager.instance.Passed();

        }
    }
}

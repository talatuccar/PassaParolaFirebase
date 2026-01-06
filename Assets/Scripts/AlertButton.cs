
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AlertButton : MonoBehaviour
{
    Button alertButton;
    public GameObject alertPanel;
    public TextMeshProUGUI alertText;
    List<TMP_InputField> inputFields;
    void Start()
    {
        alertButton = GetComponent<Button>();
        alertButton.onClick.AddListener(DeActiveAlertPanel);
    }
    void DeActiveAlertPanel()
    {
        alertPanel.SetActive(false);

        if (inputFields != null)

            SetAnswerFieldsInteractable(true, inputFields);
    }
    public void AlertPanelStatus(string message)
    {
        alertPanel.SetActive(true);
        alertText.text = message;
    }
    public void SetAnswerFieldsInteractable(bool isÝnteractable, List<TMP_InputField> givenInputFields)
    {
        inputFields = givenInputFields;

        foreach (TMP_InputField answerField in inputFields)
        {
            answerField.interactable = isÝnteractable;  // Her bir cevap InputField'ýný devre dýþý býrak
        }
    }
}

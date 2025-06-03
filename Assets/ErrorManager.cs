using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager Instance;
    public GameObject errorPopup;
    public TMP_Text errorMessageText;
    public Button ContinueBtn;

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowError(string message)
    {
        if (errorMessageText != null)
            errorMessageText.text = message;
        if (errorPopup != null)
            errorPopup.SetActive(true);
    }

    public void ClosePopUp()
    {
        errorPopup.SetActive(false);
        errorMessageText.text = "";
    }
}

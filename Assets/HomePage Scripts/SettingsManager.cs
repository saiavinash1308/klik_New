using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject Wallet;
    public GameObject Settings;
    public GameObject Feedback;

    public Button LogOut;

    private void SetAllPanelsInactive()
    {
        Wallet.SetActive(false);
        Settings.SetActive(false);
    }

    public void ActivatePanel(GameObject panelToActivate)
    {
        SetAllPanelsInactive();
        panelToActivate.SetActive(true);
    }
    public void FeedBacktoggle()
    {
        Feedback.SetActive(true);
    }

    public void onClose()
    {
        Feedback.SetActive(false );
    }

    public void OnLogOut() 
    {
        PlayerPrefs.DeleteKey("AuthToken");
        SceneManager.LoadScene("SignUp");
    }

}

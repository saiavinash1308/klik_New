using UnityEngine;

public class FooterScript : MonoBehaviour
{
    public GameObject Home;
    public GameObject Wallet;
    public GameObject SelectionPanel;
    public GameObject EditProfilePanel;
    public GameObject SettingsPanel;
    public GameObject PaymentPanel;
    public GameObject walletPanel;
    public GameObject referPanel;

    public GameObject HomeMark;
    public GameObject WalletMark;
    public GameObject ProfileMark;
    public GameObject referMark;

    void Start()
    {
        Home.SetActive(true);
        HomeMark.SetActive(true);
    }

    private void SetAllPanelsInactive()
    {
        Home.SetActive(false);
        Wallet.SetActive(false);
        SelectionPanel.SetActive(false);
        EditProfilePanel.SetActive(false);
        SettingsPanel.SetActive(false);
        PaymentPanel.SetActive(false);
        walletPanel.SetActive(false);
        referPanel.SetActive(false);
    }

    public void SetAllCheckMarksInactive()
    {
        HomeMark.SetActive(false);
        ProfileMark.SetActive(false);
        WalletMark.SetActive(false);
        referMark.SetActive(false);
    }


    public void ActivatePanel(GameObject panelToActivate)
    {
        SetAllPanelsInactive();
        panelToActivate.SetActive(true);
    }

    public void ActivateMark(GameObject MarkToActivate)
    {
        SetAllCheckMarksInactive();
        MarkToActivate.SetActive(true);
    }
}

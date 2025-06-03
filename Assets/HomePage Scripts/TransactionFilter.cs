using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static WithdrawPayment;
using System.Text;
using UnityEngine.Networking;
using System.Collections;

public class TransactionFilter : MonoBehaviour
{
    public Text TotalBalance;
    public GameObject Home;
    public GameObject allTransactionsPanel;
    public GameObject depositPanel;
    //public GameObject withdrawPanel;

    public Toggle allToggle;
    public Toggle depositToggle;
    //public Toggle withdrawToggle;

    [Header("Common Fields")]
    public TMP_InputField nameInput;
    public TMP_InputField amountInput;

    [Header("Bank Fields")]
    public TMP_InputField accountNumberInput;
    public TMP_InputField ifscInput;

    [Header("UPI Fields")]
    public TMP_InputField upiIdInput;

    //[Header("Crypto Fields")]
    //public TMP_InputField cryptoIdInput;

    [Header("Warning Popup")]
    public GameObject warningPopup;
    public TextMeshProUGUI warningText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Success PopUp")]
    public GameObject SuccessPopup;
    public Button BackToHomeBtn;

    private string pendingIfsc;

    private string pendingMethod;
    private string pendingName;
    private string pendingAmount;
    private string pendingDetail; // Account No / UPI ID / Crypto ID

    public TextMeshProUGUI errorMessageText;
    public TextMeshProUGUI MessageText;

    public HomePageManager CrashGame;
    public Button BackBtn;
    public GameObject CurrentPanel;
    public GameObject PreviousPanel;


    void Start()
    {
        BackBtn.onClick.AddListener(delegate { SetbackPanelsInactive(); });
        ActivatePanel(allTransactionsPanel);
        allToggle.isOn = true;

        allToggle.onValueChanged.AddListener(OnAllToggleChanged);
        depositToggle.onValueChanged.AddListener(OnDepositToggleChanged);
        //withdrawToggle.onValueChanged.AddListener(OnWithdrawToggleChanged);

        confirmButton.onClick.AddListener(OnConfirmWithdraw);
        cancelButton.onClick.AddListener(() => warningPopup.SetActive(false));

        errorMessageText.text = "";
        nameInput.text = "";
        amountInput.text = "";
        accountNumberInput.text = "";
        ifscInput.text = "";
        upiIdInput.text = "";
        //cryptoIdInput.text = "";
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
    if (TouchScreenKeyboard.visible)
    {
        transform.localPosition = new Vector3(0, 300, 0); // Adjust as needed
    }
    else
    {
        transform.localPosition = Vector3.zero;
    }
#endif
    }


    private void SetAllPanelsInactive()
    {
        allTransactionsPanel.SetActive(false);
        depositPanel.SetActive(false);
        //withdrawPanel.SetActive(false);
    }

    private void SetbackPanelsInactive()
    {
        CurrentPanel.SetActive(false);
        PreviousPanel.SetActive(true);
    }


    private void SetOtherTogglesInactive(Toggle activeToggle)
    {
        if (activeToggle != allToggle) allToggle.isOn = false;
        if (activeToggle != depositToggle) depositToggle.isOn = false;
        //if (activeToggle != withdrawToggle) withdrawToggle.isOn = false;
    }

    private void ActivatePanel(GameObject panel)
    {
        SetAllPanelsInactive();
        panel.SetActive(true);
    }

    private void OnAllToggleChanged(bool isOn)
    {
        if (isOn)
        {
            ActivatePanel(allTransactionsPanel);
            SetOtherTogglesInactive(allToggle);
        }
    }

    private void OnDepositToggleChanged(bool isOn)
    {
        if (isOn)
        {
            ActivatePanel(depositPanel);
            SetOtherTogglesInactive(depositToggle);
        }
    }

    //private void OnWithdrawToggleChanged(bool isOn)
    //{
    //    if (isOn)
    //    {
    //        ActivatePanel(withdrawPanel);
    //        SetOtherTogglesInactive(withdrawToggle);
    //    }
    //}

    public void OnSubmitButtonClick()
    {
        string method = GetSelectedTransactionType();
        string name = nameInput.text;
        string amount = amountInput.text;

        if (string.IsNullOrEmpty(method) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(amount))
        {
            Logger.LogWarning("Please fill all required fields.");
            ShowErrorMessage("Please fill all required fields.");
            return;
        }
        if (!decimal.TryParse(amount, out decimal amountValue) || amountValue < 1000)
        {
            Logger.LogWarning("Minimum withdrawal amount is 1000.");
            ShowErrorMessage("Minimum withdrawal amount is 1000.");
            return;
        }

        string balanceText = TotalBalance.text.Replace("₹", "").Replace(",", "").Trim();

        // Parse the total balance from UI text
        if (!decimal.TryParse(balanceText, out decimal totalBalanceValue))
        {
            Logger.LogWarning("Failed to parse total balance.");
            ShowErrorMessage("Something went wrong. Try again later.");
            return;
        }

        if (amountValue > totalBalanceValue)
        {
            Logger.LogWarning("Insufficient balance.");
            ShowErrorMessage("Insufficient balance.");
            return;
        }


        string detail = "";
        string ifsc = "";

        switch (method)
        {
            case "Bank":
                if (string.IsNullOrEmpty(accountNumberInput.text) || string.IsNullOrEmpty(ifscInput.text))
                {
                    Logger.LogWarning("Bank info missing.");
                    ShowErrorMessage("Bank info missing.");
                    return;
                }
                detail = accountNumberInput.text;
                ifsc = ifscInput.text;
                break;

            case "UPI":
                if (string.IsNullOrEmpty(upiIdInput.text))
                {
                    Logger.LogWarning("UPI ID missing.");
                    ShowErrorMessage("UPI ID missing.");
                    return;
                }
                detail = upiIdInput.text;
                break;

            //case "Crypto":
            //    if (string.IsNullOrEmpty(cryptoIdInput.text))
            //    {
            //        Logger.LogWarning("Crypto ID missing.");
            //        ShowErrorMessage("Crypto ID missing.");
            //        return;
            //    }
            //    detail = cryptoIdInput.text;
            //    break;
        }

        // Save for confirmation
        pendingMethod = method;
        pendingName = name;
        pendingAmount = amount;
        pendingDetail = detail;
        pendingIfsc = ifsc;

        // Show confirmation message
        warningText.text = $"Are you sure! You want to withdraw {amount} to your {method} account ({detail})?";
        warningPopup.SetActive(true);
    }


    private string GetSelectedTransactionType()
    {
        if (depositToggle.isOn) return "Bank";
        //if (withdrawToggle.isOn) return "Crypto";
        if (allToggle.isOn) return "UPI";
        return "None";
    }

    private void OnConfirmWithdraw()
    {
        warningPopup.SetActive(false);
        confirmButton.transform.GetChild(0).gameObject.SetActive(false);
        confirmButton.transform.GetChild(1).gameObject.SetActive(true);
        Logger.Log($"Confirmed Withdrawal - Name: {pendingName}, Amount: {pendingAmount}, Method: {pendingMethod}, To: {pendingDetail}");
        string authToken = PlayerPrefs.GetString("AuthToken");
        Logger.Log("AuthToken" + authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            Logger.LogWarning("Auth token or amount is missing.");
            Logger.LogError("Network error. Please try again.");
            ShowErrorMessage("Amount is missing.");
            return;
        }
        ShowMessage("Processing request,,,");
        StartCoroutine(SendRequest(
        authToken,
        pendingName,
        pendingAmount,
        pendingMethod,
        pendingDetail,
        pendingIfsc
    ));
        errorMessageText.text = "";
        nameInput.text = "";
        amountInput.text = "";
        accountNumberInput.text = "";
        ifscInput.text = "";
        upiIdInput.text = "";
        MessageText.text = "";
       
    }

    private IEnumerator SendRequest(string authToken, string name, string amount, string method, string account, string ifsc)
    {
        string url = "https://backend-klik.fivlog.space/api/withdrawls/create";

        UserTransaction transaction;
        if(method == "Bank")
        {
            UserTransaction temp = new UserTransaction
            {
                username = name,
                amount = float.Parse(amount),
                withdrawType = method,
                accountNumber = account,
                ifsc = ifsc
            };
            transaction = temp;
        }
        else if(method == "UPI")
        {
            UserTransaction temp = new UserTransaction
            {
                username = name,
                amount = float.Parse(amount),
                withdrawType = method,
                upi = account
            };
            transaction = temp;
        }
        else
        {
            UserTransaction temp = new UserTransaction
            {
                username = name,
                amount = float.Parse(amount),
                withdrawType = method,
                cryptoId = account
            };
            transaction = temp;
        }

        string jsonData = JsonUtility.ToJson(transaction);
        Logger.Log("Transaction JSON: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("authorization", authToken);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning("Request failed: " + request.error);
            ShowErrorMessage("Request failed. Kindly enter an amount greater than or equal to 200.");
            confirmButton.transform.GetChild(0).gameObject.SetActive(true);
            confirmButton.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            var response = JsonUtility.FromJson<ResponseMessage>(request.downloadHandler.text);
            if (response.message == "Invalid amount" || response.message == "Insufficient balance")
            {
                confirmButton.transform.GetChild(0).gameObject.SetActive(true);
                confirmButton.transform.GetChild(1).gameObject.SetActive(false);
                ShowErrorMessage(response.message);
            }
            else
            {
                Logger.Log("Request successful! Response: " + request.downloadHandler.text);
                if (float.TryParse(amount, out float floatAmount))
                {
                    CrashGame.UpdateTotalAmount(-floatAmount);
                }
                else
                {
                    Logger.LogWarning("Failed to parse withdrawal amount for update: " + amount);
                    Logger.LogError("Network error. Please try again. ");
                }

                SuccessPopup.SetActive(true);
                confirmButton.transform.GetChild(0).gameObject.SetActive(true);
                confirmButton.transform.GetChild(1).gameObject.SetActive(false);
                yield return new WaitForSeconds(2f);
            }
        }
    }

    public void OnBackToHomeClick()
    {
        SuccessPopup.SetActive(false);
        ActivatePanel(Home);
        this.gameObject.SetActive(false);
    }

    void ShowErrorMessage(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
    }

    void ShowMessage(string message)
    {
        MessageText.text = message;
        MessageText.gameObject.SetActive(true);
    }

    [System.Serializable]
    public class UserTransaction
    {
        public string username;
        public float amount;
        public string withdrawType;   // "Bank", "UPI", "Crypto"
        public string accountNumber;  // Account Number / UPI ID / Crypto ID
        public string ifsc;
        public string upi;
        public string cryptoId;

    }


}

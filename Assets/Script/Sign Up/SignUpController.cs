using UnityEngine;
using TMPro; 
using UnityEngine.Networking; 
using System.Text;
using System.Collections; 
using UnityEngine.UI;
using static PaymentScript;

public class SignUpManager : MonoBehaviour
{
    public GameObject ContinueBtn;
    public TMP_InputField nameInputField;
    public TMP_Text DeviceId;
    public TMP_InputField mobileInputField;
    public TMP_InputField referInputField;
    public GameObject Loading;
    public TextMeshProUGUI statusMessageText;
    public GameObject SignInPanel;
    public GameObject SignUpPanel;
    public GameObject OTPPanel;
    public GameObject SignInPopUp;
    public Toggle TermsToggle;
    public Toggle StateToggle;
    public GameObject termschckmark, statechckmark;
    [SerializeField]
    private bool isterms, isstate;

    //private string apiUrl = "http://localhost:3001/api/user/create"; 
    private string apiUrl = "https://backend-klik.fivlog.space/api/user/create"; 


    public void Start()
    {
        nameInputField.text = "";
        mobileInputField.text = "";
        //referInputField.text = "";
        TermsToggle.onValueChanged.AddListener(OnTermsChanged);
        StateToggle.onValueChanged.AddListener(OnStateChanged);
        termschckmark.SetActive(false);
        statechckmark.SetActive(false);
        UpdateContinueButtonState();
    }

    private void UpdateContinueButtonState()
    {
        ContinueBtn.GetComponent<Button>().interactable = isterms && isstate;
    }


    public void OnTermsChanged(bool isOn)
    {
        isterms = isOn;
        termschckmark.SetActive(isterms);
        UpdateContinueButtonState();
    }

    public void OnStateChanged(bool isOn)
    {
        isstate = isOn;
        statechckmark.SetActive(isstate);
        UpdateContinueButtonState();
    }


    public void OnSignUpButtonClicked()
    {
        if (!isterms || !isstate)
        {
            ShowStatusMessage("Please accept the Terms and Conditions and confirm your State before continuing.");
            return;
        }
        string name = nameInputField.text;
        string mobile = mobileInputField.text;
        string deviceID = DeviceId.text;
        string refer = referInputField.text;
        if (string.IsNullOrEmpty(deviceID) || deviceID == "Editor_Device_ID")
        {
            ShowStatusMessage("Authorization failed. Please use a supported device.\r\n");
            return;
        }
        if (string.IsNullOrEmpty(name)  || string.IsNullOrEmpty(mobile))
        {
            ShowStatusMessage("All fields must be filled out.");
            return;
        }

        if (name == mobile)
        {
            ShowStatusMessage("name and mobile can't be same");
            return;
        }

        ContinueBtn.transform.GetChild(0).gameObject.SetActive(false);
        ContinueBtn.transform.GetChild(1).gameObject.SetActive(true);
        Loading.gameObject.SetActive(true);

        StartCoroutine(SendSignUpRequest(name,  mobile, refer, deviceID));
        nameInputField.text = "";
        mobileInputField.text = "";
    }

    public IEnumerator SendSignUpRequest(string name, string mobile, string refer, string deviceID)
    {

        string jsonData = JsonUtility.ToJson(new UserRegistration { name = name, mobile = mobile, referralId = refer, deviceId = deviceID });

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        Logger.Log("SignUp sent:" + jsonData);
        yield return request.SendWebRequest();

        string response = request.downloadHandler.text;
        Logger.Log("Response from API: " + response);

        ApiResponse apiResponse = null;

        try
        {
            apiResponse = JsonUtility.FromJson<ApiResponse>(response);
        }
        catch
        {
            Logger.LogWarning("Failed to parse API response as JSON.");
        }

        if (apiResponse != null && apiResponse.message == "User already exists")
        {
            ShowStatusMessage("User already exists, please Sign In!");
            ContinueBtn.transform.GetChild(0).gameObject.SetActive(true);
            ContinueBtn.transform.GetChild(1).gameObject.SetActive(false);
        }
        if (apiResponse != null && apiResponse.message == "Device already registered")
        {
            ShowStatusMessage("<color=red>This device is already linked to another account.\r\n</color>");
            ContinueBtn.transform.GetChild(0).gameObject.SetActive(true);
            ContinueBtn.transform.GetChild(1).gameObject.SetActive(false);
        }
        else if (apiResponse != null && apiResponse.message == "OTP generated. Please verify.")
        {
            Logger.Log("Sign-up successful, OTP generated. Navigating to OTP scene.");
            PlayerPrefs.SetString("usermobile", mobile);
            PlayerPrefs.SetString("userName", name);
            PlayerPrefs.Save();
            ContinueBtn.transform.GetChild(0).gameObject.SetActive(true);
            ContinueBtn.transform.GetChild(1).gameObject.SetActive(false);
            SignUpPanel.SetActive(false);
            SignInPanel.SetActive(false);
            OTPPanel.SetActive(true);
        }
        else
        {
            Logger.Log("Sign-up failed with response: " + response);
            ShowStatusMessage("Sign-up failed. Please try again.");
            ContinueBtn.transform.GetChild(0).gameObject.SetActive(true);
            ContinueBtn.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    void ShowStatusMessage(string message)
    {
        statusMessageText.text = message;
        statusMessageText.gameObject.SetActive(true); 
    }

    [System.Serializable]
    public class UserRegistration
    {
        public string name;
        public string mobile;
        public string referralId;
        public string deviceId;
    }

    public void OnLoginClicked()    
    {
        SignInPanel.SetActive(true);
        SignUpPanel.SetActive(false);
        OTPPanel.SetActive(false);
    }

    public void OnSignUpClicked()
    {
        if (isterms && isstate)
        {
            SignInPanel.SetActive(true);
            SignUpPanel.SetActive(false);
            OTPPanel.SetActive(false);
            Loading.SetActive(false);
            SignInPopUp.SetActive(false);
        }

    }

    void ShowErrorMessage(string message)
    {
        statusMessageText.text = message;
    }

    private void OnDisable()
    {
        nameInputField.text = "";
        mobileInputField.text = "";
    }

}

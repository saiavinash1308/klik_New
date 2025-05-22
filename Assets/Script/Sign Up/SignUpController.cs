using UnityEngine;
using TMPro; 
using UnityEngine.Networking; 
using System.Text;
using System.Collections; 
using UnityEngine.UI;

public class SignUpManager : MonoBehaviour
{
    public GameObject ContinueBtn;
    public TMP_InputField nameInputField;
    public TMP_Text DeviceId;
    public TMP_InputField mobileInputField;
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

    private string apiUrl = "http://localhost:3001/api/user/create"; 


    public void Start()
    {
        nameInputField.text = "";
        mobileInputField.text = "";
        TermsToggle.onValueChanged.AddListener(OnTermsChanged);
        StateToggle.onValueChanged.AddListener(OnStateChanged);
        termschckmark.SetActive(false);
        statechckmark.SetActive(false);
    }

    public void OnTermsChanged(bool isOn)
    {
        isterms = isOn;  

        if (isterms)
        {
            termschckmark.SetActive(true);
        }
        else
        {
            termschckmark.SetActive(false);
        }
    }

    public void OnStateChanged(bool isOn)
    {
        isstate = isOn;  

        if (isstate)
        {
            statechckmark.SetActive(true);
        }
        else
        {
            statechckmark.SetActive(false);
        }
    }

    public void OnSignUpButtonClicked()
    {
        string name = nameInputField.text;
        string mobile = mobileInputField.text;
        string deviceID = DeviceId.text;
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

        StartCoroutine(SendSignUpRequest(name,  mobile, deviceID));
        nameInputField.text = "";
        mobileInputField.text = "";
    }

    public IEnumerator SendSignUpRequest(string name, string mobile, string deviceID)
    {
        string jsonData = JsonUtility.ToJson(new UserRegistration { name = name, mobile = mobile, deviceId = deviceID });

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string response = request.downloadHandler.text;
        Debug.Log("Response from API: " + response);

        if ((response.Contains("User already exists")))
        {
            statusMessageText.text = response;
            ShowErrorMessage("User already exists");
            SignUpPanel.SetActive(false);
            Loading.SetActive(false);
            SignInPopUp.SetActive(true);
        }
        if (response.Contains("OTP generated. Please verify."))
        {
            Debug.Log("Sign-up successful, OTP generated. Navigating to OTP scene.");
            PlayerPrefs.SetString("userName", name);
            PlayerPrefs.Save();
            Loading.SetActive(false);
            SignUpPanel.SetActive(false);
            OTPPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Sign-up failed with response: " + response);
            ShowStatusMessage("Sign-up failed: " + response);
            Loading.gameObject.SetActive(false);
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

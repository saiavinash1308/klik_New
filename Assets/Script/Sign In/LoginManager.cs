using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public GameObject SignInBtn;
    public TMP_InputField mobileInputField;
    public TextMeshProUGUI errorMessageText;
    public TextMeshProUGUI statusText;
    public GameObject SignupPanel;
    public GameObject SignInPanel;
    public GameObject OTPPanel;
    public GameObject Loading;
    public GameObject SignupPopUp;
    private string apiUrl = "http://localhost:3001/api/user/login";

    private void Start()
    {
        mobileInputField.text = "";
        SignInBtn.transform.GetChild(0).gameObject.SetActive(true);
        SignInBtn.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void OnSignInButtonClicked()
    {
        string mobile = mobileInputField.text;

        if (string.IsNullOrEmpty(mobile))
        {
            ShowErrorMessage("Mobile cannot be empty");
            return;
        }

        Loading.gameObject.SetActive(true);
        SignInBtn.transform.GetChild(0).gameObject.SetActive(false);
        SignInBtn.transform.GetChild(1).gameObject.SetActive(true);

        PlayerPrefs.SetString("mobile", mobile);
        PlayerPrefs.Save(); 

        StartCoroutine(SendLoginRequest(mobile));
        mobileInputField.text = "";
    }


    IEnumerator SendLoginRequest(string mobile)
    {
        string jsonData = JsonUtility.ToJson(new UserCredentials { mobile = mobile });
        Debug.Log("Sending JSON data: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("Request URL: " + request.url);
        Debug.Log("Request Method: " + request.method);
        Debug.Log("Response Code: " + request.responseCode);

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Connection Error: " + request.error);
            ShowErrorMessage("Connection Error Try Again");
            Loading.gameObject.SetActive(false);
            yield break;
        }
        else if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Protocol Error: " + request.error);
            Debug.LogError("Response Body: " + request.downloadHandler.text);
            SignupPopUp.SetActive(true);
            Loading.gameObject.SetActive(false);
            SignInBtn.transform.GetChild(0).gameObject.SetActive(true);
            SignInBtn.transform.GetChild(1).gameObject.SetActive(false);
            yield break;
        }
        else if (request.responseCode == 400)
        {
            string errorResponseText = request.downloadHandler.text;
            Debug.LogError("Error Response: " + errorResponseText);
            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(errorResponseText);
            ShowErrorMessage("Error: " + errorResponse.message);
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log("Response: " + responseText);
        Loading.SetActive(false);
        SignupPopUp.SetActive(false);
        SignInPanel.SetActive(false);
        SignupPanel.SetActive(false);
        SignInBtn.transform.GetChild(0).gameObject.SetActive(true);
        SignInBtn.transform.GetChild(1).gameObject.SetActive(false);
        OTPPanel.SetActive(true);
        LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);

        if (!string.IsNullOrEmpty(loginResponse.AuthToken) && loginResponse.user != null)
        {
            Debug.Log("Mobile number from response: " + mobile);
            PlayerPrefs.SetString("mobile", mobile);
            PlayerPrefs.Save();
            OTPPanel.SetActive(true);
        }
        else
        {
            //ShowErrorMessage("Incorrect Email, mobile or Password");
        }

    }

    void ShowErrorMessage(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
    }

    public void OnSignUpClicked()
    {
        SignupPanel.gameObject.SetActive(true);
        SignInPanel.gameObject.SetActive(false);
        Loading.SetActive(false);
        SignupPopUp.SetActive(false);
    }
    public void OnSignUpHere()
    {
        SignupPanel.gameObject.SetActive(false);
        SignInPanel.gameObject.SetActive(true);
        OTPPanel.gameObject.SetActive(false);
    }

    [System.Serializable]
    public class UserCredentials
    {
        public string mobile;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string AuthToken;
        public User user; 
    }

    [System.Serializable]
    public class User
    {
        public string id; 
        public string mobile;
        internal string GameID;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
    }

    private void OnDisable()
    {
        mobileInputField.text = "";
    }
}

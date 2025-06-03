using UnityEngine;
using TMPro; 
using UnityEngine.Networking; 
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;

public class OTPManager : MonoBehaviour
{
    public TMP_InputField inputFields;
    public Button submitOTPButton;
    public Button resendOTPButton;
    public TextMeshProUGUI statusText;
    public GameObject Loading;
    public GameObject CurrentPanel;
    public GameObject otpBtn;

    private string verifyOtpApiUrl = "https://backend-klik.fivlog.space/api/user/verifyotp"; 
    //private string verifyOtpApiUrl = "http://localhost:3001/api/user/verifyotp"; 
    //private string resendOtpApiUrl = "http://localhost:3001/api/user/resendotp"; 
    private string resendOtpApiUrl = "https://backend-klik.fivlog.space/api/user/resendotp"; 

    void Start()
    {
        submitOTPButton.onClick.AddListener(OnSubmitOTPClicked);
        resendOTPButton.onClick.AddListener(OnResendOTPClicked);
    }

    public void EnablePanel(GameObject panel)
    {
        panel.SetActive(true);
        CurrentPanel.SetActive(false);
    }

    public void OnSubmitOTPClicked()
    {
        string mobile = PlayerPrefs.GetString("mobile", "");
        if (string.IsNullOrEmpty(mobile))
        {
            statusText.text = "Error: Mobile not found!";
            return;
        }

        string otp = "";
        
            if (!string.IsNullOrEmpty(inputFields.text))
            {
                otp += inputFields.text;
            }
            else
            {
                statusText.text = "Error: One or more OTP fields are empty!";
                return;
            }
        

        Loading.SetActive(true);
        otpBtn.transform.GetChild(0).gameObject.SetActive(false);
        otpBtn.transform.GetChild(1).gameObject.SetActive(true);
        StartCoroutine(SubmitOTP(otp, mobile));
    }

    public void OnResendOTPClicked()
    {
        string mobile = PlayerPrefs.GetString("mobile", "");
        if (string.IsNullOrEmpty(mobile))
        {
            statusText.text = "Error: Mobile not found!";
            return;
        }

        StartCoroutine(ResendOTP(mobile));
    }

    IEnumerator SubmitOTP(string otp, string mobile)
    {
        string jsonData = JsonUtility.ToJson(new OTPRequest { mobile = mobile, otp = otp });
        Logger.Log("JSON Data: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(verifyOtpApiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Logger.LogWarning("Error: " + request.error);
            Logger.LogError("Incorrect OTP");
            Loading.SetActive(false);
            statusText.text = "Incorrect OTP";
            otpBtn.transform.GetChild(0).gameObject.SetActive(true);
            otpBtn.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            string response = request.downloadHandler.text;
            Logger.Log("Response from API: " + response);

            OTPResponse otpResponse = JsonUtility.FromJson<OTPResponse>(response);

            if (!string.IsNullOrEmpty(otpResponse.token))
            {
                PlayerPrefs.SetString("AuthToken", otpResponse.token);
                PlayerPrefs.Save();
                statusText.text = "OTP verified successfully!";
                Loading.SetActive(false);
                otpBtn.transform.GetChild(0).gameObject.SetActive(true);
                otpBtn.transform.GetChild(1).gameObject.SetActive(false);
                SceneManager.LoadScene("Home");
            }
            else
            {
                statusText.text = "OTP verification failed: " + otpResponse.message;
            }
        }
    }

    IEnumerator ResendOTP(string mobile)
    {
        string jsonData = JsonUtility.ToJson(new ResendOTPRequest { mobile = mobile });
        Logger.Log("Sending JSON data: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(resendOtpApiUrl, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Logger.LogWarning("Error: " + request.error);
            statusText.text = "Error: " + request.error;
        }
        else
        {
            string response = request.downloadHandler.text;
            Logger.Log("Response from API: " + response);

            ResendOTPResponse resendResponse = JsonUtility.FromJson<ResendOTPResponse>(response);

            if (resendResponse.success)
            {
                statusText.text = "OTP resent successfully!";
            }
            else
            {
                statusText.text = resendResponse.message;
            }
        }
    }

    [System.Serializable]
    public class OTPRequest
    {
        public string mobile;
        public string otp; 
    }

    [System.Serializable]
    public class ResendOTPRequest
    {
        public string mobile;
    }

    [System.Serializable]
    public class OTPResponse
    {
        public string token;
        public string message;
    }

    [System.Serializable]
    public class ResendOTPResponse
    {
        public bool success;
        public string message;
    }
}

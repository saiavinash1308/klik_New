using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening.Core.Easing;

public class PaymentScript : MonoBehaviour
{
    public TMP_InputField AmountInputField;
    public TextMeshProUGUI statusMessageText;
    public GameObject HomePanel;
    public GameObject CurrentPanel;
    public GameObject ProceedBtn;
    private string orderId;
    //public Button BannerBtn;

    private string apiUrl = "https://backend-klik.fivlog.space/api/transactions/create";
    private UniWebView webView;
    private bool isWebViewActive = false;
    public GameObject Successpopup;
    private float amountFloat;
    public HomePageManager CrashGame;

    private void Start()
    {
        AmountInputField.text = "0";
        statusMessageText.gameObject.SetActive(false);
        webView = GetComponent<UniWebView>();
    }


    public void EnablePanel(GameObject panel)
    {
        panel.SetActive(true);
        CurrentPanel.SetActive(false);
    }

    public void OnProceedButtonClicked()
    {
        string amount = AmountInputField.text;

        if (string.IsNullOrEmpty(amount))
        {
            ShowStatusMessage("Please enter a valid amount.");
            return;
        }

        if (float.TryParse(amount, out amountFloat))
        {
            Logger.Log("Converted float amount: " + amountFloat);

            if (amountFloat < 10 || amountFloat > 50000)
            {
                ShowStatusMessage("Please note minimum deposit is 10 and upto 50,000");
                return;
            }
        }
        else
        {
            Logger.Log("Invalid float input: " + amount);
            ShowStatusMessage("Invalid amount entered.");
            return;
        }

        ShowStatusMessage("Processing payment, please wait...");
        ProceedBtn.transform.GetChild(0).gameObject.SetActive(false);
        ProceedBtn.transform.GetChild(1).gameObject.SetActive(true);
        StartCoroutine(SendTransactionRequest((int)amountFloat));
        StartCoroutine(PaymentTimeout());
        statusMessageText.text = "";
    }

    private IEnumerator PaymentTimeout()
    {
        yield return new WaitForSeconds(300f); // 5 minutes
        if (webView != null && isWebViewActive)
        {
            CloseWebView();
            ShowStatusMessage("Payment timeout. Please try again.");
        }
    }

    public IEnumerator SendTransactionRequest(int amount)
    {
        string authToken = PlayerPrefs.GetString("AuthToken", null);
        if (string.IsNullOrEmpty(authToken))
        {
            yield break;
        }

        UserTransaction transaction = new UserTransaction
        {
            amount = amount,
        };

        string jsonData = JsonUtility.ToJson(transaction);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("authorization", authToken);

        Logger.Log("Transaction Request Data: " + jsonData);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Log("Transaction Response: " + request.downloadHandler.text);
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);

            orderId = apiResponse.orderId;
            Logger.Log("Fetched Order ID: " + orderId);

            StartRazorpayPayment(orderId, amount);
        }
        else
        {
            Logger.LogError("Transaction request failed: " + request.error);
            yield return new WaitForSeconds(2);
            StartCoroutine(SendTransactionRequest(amount));
            ProceedBtn.transform.GetChild(0).gameObject.SetActive(true);
            ProceedBtn.transform.GetChild(1).gameObject.SetActive(false);
        }
    }


    public void StartRazorpayPayment(string orderId, int amount)
    {
        webView = gameObject.AddComponent<UniWebView>();

        int toolbarHeight = Mathf.RoundToInt(Screen.height * 0.1f);

        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);

        webView.BackgroundColor = Color.white;

        webView.OnMessageReceived += OnWebViewMessageReceived;

        webView.OnShouldClose += (view) =>
        {
            CloseWebView();
            return true;
        };

        string razorpayUrl;
        if (Application.platform == RuntimePlatform.Android)
        {
            razorpayUrl = $"file:///android_asset/index.html?orderId={orderId}&amount={amount}";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            razorpayUrl = $"file://{Application.streamingAssetsPath}/index.html?orderId={orderId}&amount={amount}";
        }
        else
        {
            razorpayUrl = $"file://{Application.streamingAssetsPath}/index.html?orderId={orderId}&amount={amount}";
        }

        Logger.Log("Loading Razorpay URL: " + razorpayUrl);

        webView.Load(razorpayUrl, true);
        webView.Show();

        ProceedBtn.transform.GetChild(0).gameObject.SetActive(true);
        ProceedBtn.transform.GetChild(1).gameObject.SetActive(false);
        AmountInputField.text = "";
    }

    private void CloseWebView()
    {
        if (webView != null)
        {
            Logger.Log("🔒 Closing WebView with delay...");

            webView.Hide(true, UniWebViewTransitionEdge.None, 0.3f, () =>
            {
                Logger.Log("🧨 Destroying WebView after hiding.");
                Destroy(webView);
                webView = null;
                isWebViewActive = false;
            });
        }
    }

    public void BackToHome()
    {
        Successpopup.SetActive(false);
        CurrentPanel.SetActive(false);
        HomePanel.SetActive(true);
    }





    private void OnWebViewMessageReceived(UniWebView view, UniWebViewMessage message)
    {
        Logger.Log($"[UniWebView] Received Message: {message.RawMessage}");

        switch (message.Path)
        {
            case "payment_success":
                Logger.Log("✅ Payment success received");

                Success();

                if (view != null)
                {
                    view.Hide(true, UniWebViewTransitionEdge.None, 0.3f, () =>
                    {
                        Logger.Log("🧨 Destroying WebView after hiding...");
                        Destroy(view);
                        isWebViewActive = false;
                        webView = null;
                    });
                }
                break;

            case "payment_failed":
                Logger.LogWarning("❌ Payment failed or cancelled");


                if (view != null)
                {
                    view.Hide(true, UniWebViewTransitionEdge.None, 0.3f, () =>
                    {
                        Logger.Log("🧨 Destroying WebView after hiding...");
                        Destroy(view);
                        isWebViewActive = false;
                        webView = null;
                    });
                }
                break;

            default:
                Logger.LogWarning("⚠️ Unrecognized message path: " + message.Path);
                break;
        }
    }

    private string currentDownloadUrl = "https://klikgames.in";
    public void OnBanner()
    {
        Application.OpenURL(currentDownloadUrl);
    }






    public void Success()
    {
        Successpopup.SetActive(true);
        CrashGame.UpdateTotalAmount(amountFloat);
    }
    private void ShowStatusMessage(string message)
    {
        statusMessageText.text = message;
        statusMessageText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isWebViewActive && webView != null)
        {
            webView.Hide();
            Destroy(webView);
            isWebViewActive = false;
        }



    }

    [System.Serializable]
    public class UserTransaction
    {
        public int amount;
    }


    [System.Serializable]
    public class ApiResponse
    {
        public string message;
        public string orderId;
    }

    [System.Serializable]
    public class RazorpayResponse
    {
        public string status;
        public string paymentId;
        public string orderId;
        public string signature;
    }

    private void OnDisable()
    {
        statusMessageText.text = "";
        AmountInputField.text = "";
    }
}

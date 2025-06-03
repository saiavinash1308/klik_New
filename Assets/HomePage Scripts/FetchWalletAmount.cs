using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FetchWalletAmount : MonoBehaviour
{
    public Text[] walletAmountTexts;

    private const string apiUrl = "https://backend-klik.fivlog.space/api/wallet/getWalletAmount";
    //private const string apiUrl = "http://localhost:3001/api/wallet/getWalletAmount";
    private const string tokenKey = "AuthToken";

    void Start()
    {
        StartCoroutine(FetchWalletData());
    }

    IEnumerator FetchWalletData()
    {
        string authToken = PlayerPrefs.GetString("AuthToken", null);
        if (string.IsNullOrEmpty(authToken))
        {
            Logger.LogError("Network error. Please try again.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        request.SetRequestHeader("authorization", authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Logger.Log("Response from API: " + responseText);
            if (string.IsNullOrEmpty(responseText) || responseText == "null")
            {
                Logger.LogWarning("Empty wallet response");
                yield break;
            }

            WalletResponse walletResponse = JsonUtility.FromJson<WalletResponse>(responseText);
            if (walletResponse != null)
            {
                Logger.Log("Parsed amount: " + walletResponse.amount);

                foreach (Text walletText in walletAmountTexts)
                {
                    walletText.text = "₹ " + walletResponse.amount.ToString("N0");
                }
            }
            else
            {
                Logger.LogError("Network error. Please try again.");
            }
        }
        else
        {
            Logger.LogWarning("Error fetching wallet amount: " + request.error);
        }
    }
}

[System.Serializable]
public class WalletResponse
{
    public float amount;
}
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Networking;
//using System.Collections;

//public class FetchWalletAmount : MonoBehaviour
//{
//    public Text[] walletAmountTexts;
//    public Text[] referralIdTexts;
//    public Text bonusText;
//    private const string apiUrl = "https://backend-klik.fivlog.space/api/payments/fetchbalance";
//    //private const string apiUrl = "http://localhost:3001/api/payments/fetchbalance";
//    private const string tokenKey = "AuthToken";

//    void Start()
//    {
//        StartCoroutine(FetchWalletData());
//    }

//    IEnumerator FetchWalletData()
//    {
//        string authToken = PlayerPrefs.GetString("AuthToken", null);
//        if (string.IsNullOrEmpty(authToken))
//        {
//            Logger.LogError("Authorization token is missing.");
//            yield break;
//        }

//        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

//        request.SetRequestHeader("authorization", authToken);

//        yield return request.SendWebRequest();

//        if (request.result == UnityWebRequest.Result.Success)
//        {
//            string responseText = request.downloadHandler.text;
//            Logger.Log("Response from API: " + responseText);

//            WalletResponse walletResponse = JsonUtility.FromJson<WalletResponse>(responseText);
//            if (walletResponse != null)
//            {
//                Logger.Log("Parsed amount: " + walletResponse.balance);
//                Logger.Log("Parsed amount: " + walletResponse.bonus);

//                foreach (Text text in walletAmountTexts)
//                {
//                    if (text != null)
//                        text.text = "₹ " + walletResponse.balance.ToString();
//                }

//                foreach (Text text in referralIdTexts)
//                {
//                    if (text != null)
//                        text.text = walletResponse.referralId;
//                }
//                if (bonusText != null)
//                    bonusText.text = "₹ " + walletResponse.bonus.ToString();

//            }
//            else
//            {
//                Logger.LogError("Failed to parse wallet response.");
//            }
//        }
//        else
//        {
//            Logger.LogError("Error fetching wallet amount: " + request.error);
//        }
//    }
//}

//[System.Serializable]
//public class WalletResponse
//{
//    public float balance;
//    public float bonus;
//    public string referralId;
//}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FetchWalletAmount : MonoBehaviour
{
    public Text[] walletAmountTexts; 

    private const string apiUrl = "http://localhost:3001/api/wallet/getWalletAmount"; 
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
            Debug.LogError("Authorization token is missing.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        request.SetRequestHeader("authorization", authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Response from API: " + responseText);

            WalletResponse walletResponse = JsonUtility.FromJson<WalletResponse>(responseText);
            if (walletResponse != null)
            {
                Debug.Log("Parsed amount: " + walletResponse.amount);  

                foreach (Text walletText in walletAmountTexts)
                {
                    walletText.text = "₹ " + walletResponse.amount.ToString("N0"); 
                }
            }
            else
            {
                Debug.LogError("Failed to parse wallet response.");
            }
        }
        else
        {
            Debug.LogError("Error fetching wallet amount: " + request.error);
        }
    }
}

[System.Serializable]
public class WalletResponse
{
    public float amount;
}

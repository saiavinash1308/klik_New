using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class Wallet : MonoBehaviour
{
    public GameObject PaymentPanel;
    public GameObject WithdrawlPaymentPanel;
    public GameObject CurrentPanel;

    public GameObject transactionDataPrefab;  
    public Transform contentPanel1;

    private const string baseUrl = "https://backend-klik.fivlog.space/api/transactions/fetchTransactions";

    void Start()
    {
        CurrentPanel.SetActive(true);
        StartCoroutine(GetTransactionData());
        
    }

    public void EnablePanel(GameObject panel)
    {
        panel.SetActive(true);
        CurrentPanel.SetActive(false);
    }

    private IEnumerator GetTransactionData()
    {
        Logger.Log($"Sending GET request to: {baseUrl}");

        string authToken = PlayerPrefs.GetString("AuthToken", null);
        if (string.IsNullOrEmpty(authToken))
        {
            Logger.LogError("Network error. Please try again.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(baseUrl);  
        request.SetRequestHeader("authorization", authToken);  
        yield return request.SendWebRequest(); 

        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning($"Error fetching transactions: {request.error}");
            Logger.LogError($"Network error. Please try again.");
            yield break;
        }

        string rawResponse = request.downloadHandler.text;
        Logger.Log($"Backend Response: {rawResponse}");

        if (string.IsNullOrEmpty(rawResponse))
        {
            Logger.LogWarning("Empty response from transactions API.");
            yield break;
        }

        TransactionResponseWrapper responseWrapper = JsonUtility.FromJson<TransactionResponseWrapper>("{\"transactions\":" + rawResponse + "}");

        if (responseWrapper.transactions == null || responseWrapper.transactions.Count == 0)
        {
            Logger.Log("No transactions available.");
            yield break;
        }

        Logger.Log("Parsed Transaction Data:");
        foreach (var transaction in responseWrapper.transactions)
        {
            Logger.Log($"Order ID: {transaction.orderId}, Amount: {transaction.amount}, Status: {transaction.status}, CreatedAt: {transaction.createdAt}");
        }

        PopulatePanel(contentPanel1, responseWrapper.transactions);

        Canvas.ForceUpdateCanvases();
    }

    private void PopulatePanel(Transform contentPanel, List<Transaction> transactions)
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var transaction in transactions)
        {
            GameObject transactionDataUI = Instantiate(transactionDataPrefab, contentPanel);

            TMP_Text[] textComponents = transactionDataUI.GetComponentsInChildren<TMP_Text>();

            textComponents[0].text = transaction.orderId;    
            textComponents[1].text = $"Amount: {transaction.amount}"; 
            textComponents[2].text = $"Status: {transaction.status}";  

            DateTime createdAt = DateTime.Parse(transaction.createdAt);
            textComponents[3].text = $"Created: {createdAt.ToString("yyyy-MM-dd HH:mm:ss")}";  

            TMP_Text amountText = textComponents[1];

            if (transaction.status == "Pending")
            {
                amountText.color = Color.yellow;  
            }
            else if (transaction.status == "Paid")
            {
                amountText.color = Color.green;   
            }
            else if (transaction.status == "Failed")
            {
                amountText.color = Color.red;     
            }
        }
    }

    [System.Serializable]
    public class Transaction
    {
        public string orderId;    
        public string createdAt;  
        public float amount;      
        public string status;     
    }

    [System.Serializable]
    public class TransactionResponseWrapper
    {
        public List<Transaction> transactions;  
    }
}

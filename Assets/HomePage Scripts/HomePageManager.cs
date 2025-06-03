using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Globalization;
using System.Text.RegularExpressions;

public class HomePageManager : MonoBehaviour
{
    private SocketManager socketManager;
    public Text totalAmount;
    public Text DeposittotalAmount;
    public Text WithdrawtotalAmount;
    [SerializeField]
    private GameObject Selectscreen;
    [SerializeField]
    private GameObject ProfilePanel;

    [SerializeField]
    private GameObject currentPanel;
    [SerializeField]
    private GameObject HomePanel;
    [SerializeField]
    private GameObject EditPanel;

    [SerializeField]
    private GameObject MindMorgaLoadingPanel;

    [SerializeField]
    private GameObject LudoLoadingPanel;

    public GameObject HomeMark;
    public GameObject WalletMark;
    public GameObject LeaderBoardMark;

    [SerializeField]
    GameFetcherScript gameFetcher;

    public Sprite[] avatarSprites;
    public Image[] avatarPreviewImages;
    private const string baseUrl = "https://sockets-klik.fivlog.space/";
    public TMP_Text userCountText;


    void Start()
    {
        int savedIndex = PlayerPrefs.GetInt("SelectedAvatar", 0);
        Sprite selectedAvatar = avatarSprites[savedIndex];

        // Apply to all preview UI images
        foreach (Image img in avatarPreviewImages)
        {
            img.sprite = selectedAvatar;
        }
        socketManager = FindObjectOfType<SocketManager>();
        if (socketManager == null)
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }

        HomeMark.SetActive(true);
        WalletMark.SetActive(false);
        StartCoroutine(GetUserCount());
    }

    public void OnProfile()
    {
        ShowPanel(ProfilePanel);
        Logger.LogWarning("Profile Panel is active");
    }
    public void OnEditProfile()
    {
        ShowPanel(EditPanel);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        if (currentPanel != null)
        {
            currentPanel.SetActive(false); 
        }
        panel.SetActive(true); 
        currentPanel = panel;
    }

    public void OnBackButtonClicked()
    {
       HomePanel.SetActive(true);
        HomeMark.SetActive(true);
        ProfilePanel.SetActive(false);
        Selectscreen.SetActive(false);
    }
    public void OnMindMorga()
    {
        ShowPanel(MindMorgaLoadingPanel);
        if (gameFetcher != null)
        {
            StartCoroutine(gameFetcher.GetGameData("MEMORYGAME"));
        }
        else
        {
            Logger.LogWarning("GameFetcherScript reference is missing!");
        }
    }

    public void OnFastLudoBtn()
    {
        ShowPanel(LudoLoadingPanel);
        if (gameFetcher != null)
        {
            StartCoroutine(gameFetcher.GetGameData("LUDO"));
        }
        else
        {
            Logger.LogWarning("GameFetcherScript reference is missing!");
        }
    }

    internal void UpdateTotalAmount(float value)
    {

        try
        {
            string raw = totalAmount.text;
            string cleaned = Regex.Replace(raw, @"[^\d\.\-]", "");
            if (float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out float currentTotal))
            {
                currentTotal += value;
                totalAmount.text = "₹ " + currentTotal.ToString("0.00", CultureInfo.InvariantCulture);
                DeposittotalAmount.text = "₹ " + currentTotal.ToString("0.00", CultureInfo.InvariantCulture);
                WithdrawtotalAmount.text = "₹ " + currentTotal.ToString("0.00", CultureInfo.InvariantCulture);
            }
            else
            {
                Logger.LogWarning($"Invalid total amount format! raw=\"{raw}\" cleaned=\"{cleaned}\"");
            }

        }
        catch
        {
            Logger.LogError($"Failed to update amount");
        }
    }
    private IEnumerator GetUserCount()
    {
        Logger.Log("Getting user count");
        UnityWebRequest request = UnityWebRequest.Get(baseUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning($"Error fetching user count: {request.error}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Logger.Log($"Backend Response: {responseText}");

        UserCountResponse response = JsonUtility.FromJson<UserCountResponse>(responseText);

        // Update the UI text with the user count
        userCountText.text = $" {response.count}";
    }

    [System.Serializable]
    public class UserCountResponse
    {
        public int count;
    }
}
            
                
                


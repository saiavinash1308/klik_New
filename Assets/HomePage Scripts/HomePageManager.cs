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
            Debug.LogError("SocketManager not found!");
            return;
        }

        HomeMark.SetActive(true);
        WalletMark.SetActive(false);
    }

    public void OnProfile()
    {
        ShowPanel(ProfilePanel);
        Debug.LogWarning("Profile Panel is active");
    }
    public void OnEditProfile()
    {
        ShowPanel(EditPanel);
    }

    private void ShowPanel(GameObject panel)
    {
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
            Debug.LogError("GameFetcherScript reference is missing!");
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
            Debug.LogError("GameFetcherScript reference is missing!");
        }
    }

    internal void UpdateTotalAmount(float value)
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
            Debug.LogError($"Invalid total amount format! raw=\"{raw}\" cleaned=\"{cleaned}\"");
        }
    }
}
            
                
                


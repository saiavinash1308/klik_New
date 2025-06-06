using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditUserProfile : MonoBehaviour
{
    private string APIIUrl = "https://backend-klik.fivlog.space/api/auth/profile";
    //public string APIIUrl = "http://localhost:3001/api/auth/profile";

    public TextMeshProUGUI nameInput;
    public Button saveButton;
    public Button BackBtn;
    public GameObject CurrentPanel;
    public GameObject PreviousPanel;
    public Button editButton;
    public GameObject PictureUI;
    public Sprite[] avatarSprites;          
    public Image [] previewImages;               
    private int selectedIndex = 0;

    

    void Start()
    {
        saveButton.onClick.AddListener(UpdateProfile);
        editButton.onClick.AddListener(Profile);
        BackBtn.onClick.AddListener(delegate { SetAllPanelsInactive(); });
    }

    public void SelectAvatar(int index)
    {
        selectedIndex = index;

        foreach (Image img in previewImages)
        {
            img.sprite = avatarSprites[selectedIndex];
        }
        PictureUI.SetActive(false);
    }

    public void Profile()
    {
        PictureUI.SetActive(true);
    }

    public void UpdateProfile()
    {
        PlayerPrefs.SetInt("SelectedAvatar", selectedIndex);
        PlayerPrefs.Save();
        string userToken = PlayerPrefs.GetString("AuthToken", null);

        if (string.IsNullOrEmpty(userToken))
        {
            Logger.LogError("Network error. Please try again.");
            return;
        }

        //StartCoroutine(UpdateUserData());
    }

    IEnumerator UpdateUserData()
    {
        Logger.Log("Name Input: " + nameInput.text);
        var userData = new
        {
            name = nameInput.text,
        };
        string json = JsonUtility.ToJson(userData);
        Logger.Log("Serialized JSON: " + json);

        using (UnityWebRequest request = UnityWebRequest.Put(APIIUrl, json))
        {
            //request.SetRequestHeader("Authorization", "Bearer " + userToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogWarning("Error updating user profile: " + request.error + " | Response Code: " + request.responseCode);
            }
            else
            {
                Logger.Log("User profile updated successfully: " + request.downloadHandler.text);
            }
        }
    }


    private void SetAllPanelsInactive()
    {
        CurrentPanel.SetActive(false);
        PreviousPanel.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Image profilePicture;
    private string playerId;
    public PawnType playerPawn;

    private void Start()
    {
        profilePicture = GetComponent<Image>();   
    }
    public void SetProfile(string profilePic, string playerId)
    {
        print("setProfile");
        this.playerId = playerId;

        bool invalidPic = string.IsNullOrEmpty(profilePic) || profilePic.Equals("noProfile");

        if (invalidPic)
        {
            Debug.LogWarning($"Profile picture is invalid or missing for player {playerId}");
            return;
        }

        try
        {
            if (!IsBase64String(profilePic))
            {
                Debug.LogWarning($"Profile picture string is not valid Base64 for player {playerId}");
                return;
            }

            byte[] imageData = System.Convert.FromBase64String(profilePic);
            Texture2D tex = new Texture2D(2, 2);

            if (tex.LoadImage(imageData))
            {
                profilePicture.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning($"Failed to load image from Base64 for player {playerId}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error decoding profile picture for player {playerId}: {ex.Message}");
        }
    }
    private bool IsBase64String(string s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s) || s.Length % 4 != 0)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,2}$");
    }


    public void SetDefaultProfile()
    {
        profilePicture.sprite = UiManager.instance.defaultProfilePic;
    }


}

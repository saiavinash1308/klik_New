using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FetchUserProfile : MonoBehaviour
{
    public Text[] usernameTextArray; 
    public Text[] mobileTextArray;   
    public Text totalMatchesText;
    public Text matchesWonText;
    public Text matchesLostText;

    private const string apiUrl = "https://backend-klik.fivlog.space/api/user/profile"; 
    //private const string apiUrl = "http://localhost:3001/api/user/profile"; 
    private const string tokenKey = "AuthToken"; 

    void Start()
    {
        StartCoroutine(FetchProfileData());
    }

    IEnumerator FetchProfileData()
    {
        string authToken = PlayerPrefs.GetString(tokenKey, null);
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

            ProfileResponse profileResponse = JsonUtility.FromJson<ProfileResponse>(responseText);

            if (profileResponse != null && profileResponse.user != null)
            {
                Debug.Log("Parsed Username: " + profileResponse.user.username);
                Debug.Log("Parsed Mobile: " + profileResponse.user.mobile);
                Debug.Log("Parsed Total Matches: " + profileResponse.user.totalMatches);
                Debug.Log("Parsed Matches Won: " + profileResponse.user.matchesWon);

                int matchesLost = profileResponse.user.totalMatches - profileResponse.user.matchesWon;

                foreach (Text usernameText in usernameTextArray)
                {
                    usernameText.text = "" + profileResponse.user.username;
                }

                foreach (Text mobileText in mobileTextArray)
                {
                    mobileText.text = "" + profileResponse.user.mobile;
                }

                totalMatchesText.text = "" + profileResponse.user.totalMatches.ToString();
                matchesWonText.text = "" + profileResponse.user.matchesWon.ToString();
                matchesLostText.text = "" + matchesLost.ToString(); 
            }
            else
            {
                Debug.LogError("Failed to parse profile response.");
            }
        }
        else
        {
            Debug.LogError("Error fetching profile data: " + request.error);
        }
    }
}

[System.Serializable]
public class ProfileResponse
{
    public UserProfile user;
}

[System.Serializable]
public class UserProfile
{
    public string username;
    public string mobile;
    public int totalMatches;
    public int matchesWon;
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class SplashScreenController : MonoBehaviour
{
    public float splashDuration = 3f;  

    void Start()
    {
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(splashDuration);

        string userName = PlayerPrefs.GetString("AuthToken", null);  

        if (!string.IsNullOrEmpty(userName))
        {
            Logger.Log("User is logged in: " + userName);
            SceneManager.LoadScene("Home"); 
        }
        else
        {
            Logger.Log("User is not logged in.");
            SceneManager.LoadScene("SignUp"); 
        }
    }
}

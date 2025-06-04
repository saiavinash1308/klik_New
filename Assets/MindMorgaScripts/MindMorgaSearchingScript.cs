using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MindMorgaSearchScript : MonoBehaviour
{
    public bool isSearching;
    public Image loadingImage;
    public static MindMorgaSearchScript Searching { get; private set; }
    private SocketManager socketManager;
    public GameObject Loading;
    public GameObject PopUp;
    public Button Back;
    public Button Quit;
    public Button Cancle;
    public Sprite[] avatarSprites;
    public Image[] avatarPreviewImages;

    public Text timerText;

    private float searchDuration = 50f;

    void Awake()
    {
        if (Searching != null && Searching != this)
        {
            Destroy(Searching.gameObject);
        }

        Searching = this;
        socketManager = FindObjectOfType<SocketManager>();
    }


    void Start()
    {
        if (socketManager == null)
        {
            Logger.LogError("Network error. Please try again.");
            return; // Exit early to avoid null reference later
        }
       
        int savedIndex = PlayerPrefs.GetInt("SelectedAvatar", 0);
        Sprite selectedAvatar = avatarSprites[savedIndex];

        // Apply to all preview UI images
        foreach (Image img in avatarPreviewImages)
        {
            img.sprite = selectedAvatar;
        }

        isSearching = true;
        StartCoroutine(SearchAndLoadCoroutine());
        StartCoroutine(SearchTimeoutTimer());
    }

    private IEnumerator SearchTimeoutTimer()
    {
        float timeLeft = searchDuration;

        while (timeLeft > 0f && isSearching)
        {
            timeLeft -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(timeLeft);
            timerText.text = seconds.ToString();
            yield return null;
        }

        if (isSearching)
        {
            Logger.LogWarning("Search timeout reached. Redirecting to fallback scene.");
            SceneManager.LoadScene("Home"); // or another fallback scene like "NoMatchFound"
        }
    }

    private IEnumerator SearchAndLoadCoroutine()
    {
        //loadingImage.gameObject.SetActive(true); // Show loading image
        //Loading.gameObject.SetActive(true);
        while (socketManager != null && socketManager.stopSearch)
        {
            loadingImage.fillAmount = Mathf.PingPong(Time.time, 1f); // Smooth fill between 0 and 1

            yield return new WaitForSeconds(2f);
        }

        loadingImage.gameObject.SetActive(false);
        Loading.gameObject.SetActive(false);
        SceneManager.LoadScene("MindMorga");
    }
    public void StopSearching()
    {
        Logger.LogWarning("Searching Stopped...");
        isSearching = false;
    }

    public void EnablePopUp()
    {
        PopUp.SetActive(true);
    }

    public void QuitToHome()
    {
        if (socketManager != null && socketManager.isConnected)
        {
            socketManager.socket.Emit("QUIT_GAME", " ");
            Logger.Log("Sent game quit");
            SceneManager.LoadScene("Home");
        }
        else
        {
            Logger.LogWarning("Socket is not connected. Cannot send game ID.");
        }
    }
    public void ClosePopup()
    {
        if (PopUp != null)
        {
            PopUp.SetActive(false);
        }
    }

}

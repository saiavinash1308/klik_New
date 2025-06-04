using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class YouWinPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI youWinText;
    [SerializeField] private float delayBeforeLoad = 4f; // Delay in seconds
    [SerializeField] private string sceneToLoad = "Home"; // Set this in Inspector or hardcode

    public void OnPlayerWin(PawnType lftPawn)
    {
        gameObject.SetActive(true);
        youWinText.text = lftPawn.ToString() + " left you win";

        // Start the coroutine to load scene after delay
        StartCoroutine(LoadSceneAfterDelay());
    }

    private void OnDisable()
    {
        youWinText.text = "you win";
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}

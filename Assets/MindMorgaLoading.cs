using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindMorgaLoading : MonoBehaviour
{
    public float splashDuration = 2f;
    public GameObject SelectScene;
    public GameObject CurrentPanel;

    private void OnEnable() 
    {
        ResetLoadingPanel();
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private void ResetLoadingPanel()
    {
        SelectScene.SetActive(false); 
        CurrentPanel.SetActive(true); 
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        float elapsedTime = 0f;

        while (elapsedTime < splashDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SelectScene.SetActive(true);
        CurrentPanel.SetActive(false);
    }
}

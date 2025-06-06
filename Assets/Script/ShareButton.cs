using System.Collections;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI; // Only if using TextMeshPro

public class ShareButton : MonoBehaviour
{
    public Text referralText; // Assign this in the Inspector
    public Button BackBtn;
    public GameObject CurrentPanel;
    public GameObject PreviousPanel;

    private void Start()
    {
        BackBtn.onClick.AddListener(delegate { SetAllPanelsInactive(); });
    }
    public void ClickShareBtn()
    {
        StartCoroutine(APKShare(referralText.text));
    }

    private IEnumerator APKShare(string referralMessage)
    {
        yield return new WaitForEndOfFrame();

        Texture2D image = Resources.Load("image", typeof(Texture2D)) as Texture2D;

        if (image == null)
        {
            Logger.LogWarning("Failed to load texture from Resources.");
            yield break;
        }

        Texture2D readableTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        readableTexture.SetPixels(image.GetPixels());
        readableTexture.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared_img.png");
        File.WriteAllBytes(filePath, readableTexture.EncodeToPNG());

        Destroy(readableTexture);


        string finalMessage = string.IsNullOrWhiteSpace(referralMessage)
        ? "Referal feature coming soon\nDownload from here: https://klikgames.in"
        : "Use my referral code: " + referralMessage + "\nDownload from here: https://klikgames.in";

        new NativeShare()
            .AddFile(filePath)
            .SetSubject("Klik Games")
            .SetText(finalMessage)
            .SetCallback((result, shareTarget) => Logger.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();


    }
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = referralText.text;
        Logger.Log("Copied to clipboard: " + referralText.text);
    }

    private void SetAllPanelsInactive()
    {
        CurrentPanel.SetActive(false);
        PreviousPanel.SetActive(true);
    }
}

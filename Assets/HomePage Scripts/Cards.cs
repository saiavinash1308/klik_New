using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BannerLoader : MonoBehaviour
{
    private string apiUrl = "https://backend-klik.fivlog.space/banner/fetchallbanners";
    //private string apiUrl = "http://localhost:3001/api/banner/fetchallbanners";
    private string baseImageUrl = ""; 

    public GameObject imagePrefab; 
    public Transform contentPanel; 

    public float fixedHeight = 100f; 
    public float spacing = 10f; 

    [System.Serializable]
    public class Banner
    {
        public string imageUrl;
    }

    [System.Serializable]
    public class BannerList
    {
        public Banner[] banners;
    }

    void Start()
    {
        StartCoroutine(LoadBannersFromAPI());
    }

    IEnumerator LoadBannersFromAPI()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error while fetching banners: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("API Response: " + jsonResponse);

                BannerList bannerList = JsonUtility.FromJson<BannerList>(jsonResponse);
                if (bannerList == null || bannerList.banners == null || bannerList.banners.Length == 0)
                {
                    Debug.LogError("No banners found in the API response.");
                    yield break;
                }

                float totalWidth = 0;
                Texture2D[] textures = new Texture2D[bannerList.banners.Length]; 

                for (int i = 0; i < bannerList.banners.Length; i++)
                {
                    string fullImageUrl = bannerList.banners[i].imageUrl; 
                    Debug.Log("Downloading image from: " + fullImageUrl);

                    yield return StartCoroutine(DownloadTexture(fullImageUrl, (texture) =>
                    {
                        textures[i] = texture;
                        if (texture != null)
                        {
                            float aspectRatio = (float)texture.width / texture.height;
                            float width = fixedHeight * aspectRatio;
                            totalWidth += width + spacing;
                        }
                    }));
                }

                RectTransform contentPanelRect = contentPanel.GetComponent<RectTransform>();
                contentPanelRect.sizeDelta = new Vector2(totalWidth, fixedHeight);

                float currentX = 0; 
                for (int i = 0; i < bannerList.banners.Length; i++)
                {
                    Texture2D texture = textures[i];
                    if (texture != null)
                    {
                        yield return StartCoroutine(DownloadAndDisplayBanner(bannerList.banners[i].imageUrl, texture, currentX));
                        currentX += fixedHeight * (float)texture.width / texture.height + spacing; 
                    }
                }
            }
        }
    }

    IEnumerator DownloadTexture(string imageUrl, System.Action<Texture2D> onTextureDownloaded)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error while downloading image: " + request.error + " from URL: " + imageUrl);
                onTextureDownloaded(null);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                onTextureDownloaded(texture);
            }
        }
    }

    IEnumerator DownloadAndDisplayBanner(string imageUrl, Texture2D texture, float xPosition)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null. Skipping display.");
            yield break;
        }

        if (imagePrefab == null)
        {
            Debug.LogError("Image Prefab is not assigned.");
            yield break;
        }

        GameObject newBanner = Instantiate(imagePrefab, contentPanel);

        if (newBanner == null)
        {
            Debug.LogError("Failed to instantiate banner prefab.");
            yield break;
        }

        RectTransform rectTransform = newBanner.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform component not found on the instantiated banner.");
            yield break;
        }

        float aspectRatio = (float)texture.width / texture.height;
        float width = fixedHeight * aspectRatio; 

        rectTransform.sizeDelta = new Vector2(width, fixedHeight);
        rectTransform.anchoredPosition = new Vector2(xPosition, 0); 

        Image bannerImage = newBanner.GetComponent<Image>();
        if (bannerImage == null)
        {
            Debug.LogError("Image component not found in prefab.");
            yield break;
        }
        bannerImage.sprite = SpriteFromTexture2D(texture);
    }

    private Sprite SpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    public TMP_Text DeviceId;
    public TMP_Text AppsListText;

    void Start()
    {
        string deviceId = GetAndroidId();
        DeviceId.text = deviceId;
        Debug.Log("Real Android ID: " + deviceId);
        PlayerPrefs.SetString("DeviceID", deviceId);
        PlayerPrefs.Save();

        List<string> apps = GetInstalledApps();
        AppsListText.text = string.Join("\n", apps);
    }

    public static string GetAndroidId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
            string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
            return androidId;
        }
#else
        return "Editor_Device_ID";
#endif
    }

    public static List<string> GetInstalledApps()
    {
        List<string> appList = new List<string>();

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject apps = packageManager.Call<AndroidJavaObject>("getInstalledApplications", 0);

            int size = apps.Call<int>("size");
            for (int i = 0; i < size; i++)
            {
                AndroidJavaObject appInfo = apps.Call<AndroidJavaObject>("get", i);
                string packageName = appInfo.Get<string>("packageName");
                appList.Add(packageName);
            }
        }
#endif

        return appList;
    }
}

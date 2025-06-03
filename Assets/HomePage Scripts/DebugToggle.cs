using UnityEngine;

public class LoggerToggle : MonoBehaviour
{
    public bool enableLogging = true;

    void Awake()
    {
        Logger.EnableLogging = enableLogging;
    }
}

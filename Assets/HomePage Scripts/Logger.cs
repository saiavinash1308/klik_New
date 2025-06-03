using UnityEngine;

public static class Logger
{
    public static bool EnableLogging = true;

    public static void Log(string message)
    {
        if (EnableLogging)
            Debug.Log(message);
    }

    public static void LogWarning(string message)
    {
        if (EnableLogging)
            Debug.LogWarning(message);
    }

    public static void LogError(string message)
    {
        if (EnableLogging)
            Debug.LogError(message);

        if (ErrorManager.Instance != null)
            ErrorManager.Instance.ShowError(message);
    }


    // Optional: overloads for objects or format strings
    public static void Log(string format, params object[] args)
    {
        if (EnableLogging)
            Debug.LogFormat(format, args);
    }
}

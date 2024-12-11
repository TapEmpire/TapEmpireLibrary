using System;
using UnityEngine;

public static class MonetizationLogger
{
    static AndroidJavaClass m_logger = new AndroidJavaClass("com.kokozone.device.KZMonetization");

    public static void Log(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_logger.CallStatic("logMessage", message);
#else
        //Debug.Log("[Monetization] " + message);
#endif
    }
    public static void LogWarning(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_logger.CallStatic("logWarning", message);
#else
        Debug.LogWarning("[Monetization] " + message);
#endif
    }
    public static void LogError(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_logger.CallStatic("logError", message);
#else
        Debug.LogError("[Monetization] " + message);
#endif
    }
    public static void LogException(string message, Exception e)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            LogError(message);
            LogError(e.StackTrace);
#else
        LogError("[Monetization] " + message);
        LogError(e.StackTrace);
#endif
    }
}
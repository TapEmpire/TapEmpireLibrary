using UnityEngine;
using System.Runtime.InteropServices;

public static class MobileToast
{
    public static void Show(string message, bool isLong)
    {
#if UNITY_EDITOR
        Debug.Log("Toast : " + message);
#elif UNITY_ANDROID
        try
            {
                var toastClass = new AndroidJavaClass("android.widget.Toast");
                var context = GetUnityActivity();
                if (context != null)
                {
                    var javaToastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", context, message, toastClass.GetStatic<int>(isLong ? "LENGTH_LONG" : "LENGTH_SHORT"));
                    javaToastObject.Call("show");
                }
            }
            catch (System.Exception) { }
#elif UNITY_IOS
        showToast(message, isLong);
#endif
    }

#if UNITY_ANDROID
    static AndroidJavaObject GetUnityActivity()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

#if UNITY_IOS
        [DllImport ("__Internal")]
		private static extern void showToast(string msg, bool isLong);
#endif
}
using System.Runtime.InteropServices;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public static class MobileToast
    {
        public static void Show(string message, bool isLong)
        {
#if UNITY_EDITOR
            Debug.Log($"Toast: {message}");
#elif UNITY_ANDROID
            try
            {
                using var toastClass = new AndroidJavaClass("android.widget.Toast");
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (context == null) return;
                var duration = toastClass.GetStatic<int>(isLong ? "LENGTH_LONG" : "LENGTH_SHORT");
                using var toast = toastClass.CallStatic<AndroidJavaObject>("makeText", context, message, duration);
                toast.Call("show");
            }
            catch
            {
                // Best-effort; toast failures are non-fatal.
            }
#elif UNITY_IOS
            showToast(message, isLong);
#endif
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void showToast(string msg, bool isLong);
#endif
    }
}

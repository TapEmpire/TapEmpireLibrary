using System.Threading;
using Cysharp.Threading.Tasks;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace TapEmpire.Services
{
    public static class AdvertisingId
    {
        public static UniTask<string> Get(CancellationToken cancellationToken = default)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UniTask.RunOnThreadPool(() =>
            {
                // JNI calls require the worker thread to be attached to the JVM;
                // AdvertisingIdClient.getAdvertisingIdInfo must not run on the main thread.
                AndroidJNI.AttachCurrentThread();
                try
                {
                    using var advertisingIdClass = new AndroidJavaClass("com.tapempire.ads.AdvertisingId");
                    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    return advertisingIdClass.CallStatic<string>("getAdvertisingId", context);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    AndroidJNI.DetachCurrentThread();
                }
            }, cancellationToken: cancellationToken);
#else
            return UniTask.FromResult<string>(null);
#endif
        }
    }
}

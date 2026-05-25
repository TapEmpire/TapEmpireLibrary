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
            }, cancellationToken: cancellationToken);
#else
            return UniTask.FromResult<string>(null);
#endif
        }
    }
}

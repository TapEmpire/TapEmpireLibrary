using System;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    internal class AdSessionGuardModule : IDisposable
    {
        private const string NativeClassName = "com.tapempire.ads.AdSessionGuard";

        private readonly CompositeDisposable _disposables = new();

        public AdSessionGuardModule(SystemSettings systemSettings, IInterstitial interstitial, IRewarded rewarded)
        {
            InitNative((long)systemSettings.SessionInterval);

            systemSettings.OnDataChanged
                .Subscribe(settings => SetTimeoutNative((long)settings.SessionInterval))
                .AddTo(_disposables);

            Observable<bool> interstitialShowing = interstitial?.IsShowing ?? Observable.Return(false);
            Observable<bool> rewardedShowing = rewarded?.IsShowing ?? Observable.Return(false);

            interstitialShowing
                .CombineLatest(rewardedShowing, (isInterstitialShowing, isRewardedShowing) => isInterstitialShowing || isRewardedShowing)
                .DistinctUntilChanged()
                .Subscribe(SetAdActiveNative)
                .AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        private static void InitNative(long timeoutSeconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var guardClass = new AndroidJavaClass(NativeClassName);
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            guardClass.CallStatic("init", activity, timeoutSeconds);
#endif
        }

        private static void SetTimeoutNative(long timeoutSeconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var guardClass = new AndroidJavaClass(NativeClassName);
            guardClass.CallStatic("setTimeout", timeoutSeconds);
#endif
        }

        private static void SetAdActiveNative(bool active)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var guardClass = new AndroidJavaClass(NativeClassName);
            guardClass.CallStatic("setAdActive", active);
#endif
        }
    }
}

using System;
using System.Collections.Generic;
using TapEmpire.Services;
using UnityEngine;

namespace TapEmpire.Experimental
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/v3/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        public AdsConfig Config;

        [Header("Banner / MREC positioning")]
        public AdPosition BannerPosition = AdPosition.BottomCenter;
        public BannerWidth BannerSize = BannerWidth.Full;
        public AdPosition MrecPosition = AdPosition.BottomCenter;

        [Header("Show toggles")]
        public bool EnableBanner = true;
        public bool EnableMrec = true;
        public bool EnableInterstitial = true;

        [Header("Level gating")]
        public int FromLevel = 0;
        public int BannerFromLevel = 0;
        public int RewardedFromLevel = 0;

        [Header("Interstitial scheduling")]
        public List<TimerData> TimerData = new();

        [Header("Scene settings")]
        public List<SceneSettings> SceneSettings = new();

        [Header("Analytics")]
        public AdsAnalyticsSettings AdsAnalyticsSettings;

        [Header("Other")]
        public bool ShowApplovinOn2GB;
    }

    public enum BannerWidth
    {
        Full,
        Half,
    }

    [Serializable]
    public struct TimerData
    {
        public int FromLevel;
        public int Seconds;
    }

    [Serializable]
    public struct SceneSettings
    {
        public SceneName SceneName;
        public bool IsBannerEnabled;
    }
}

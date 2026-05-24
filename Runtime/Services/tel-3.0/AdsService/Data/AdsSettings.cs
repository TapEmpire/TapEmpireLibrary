using System;
using System.Collections.Generic;
#if TEL_METICA
using Metica.Unity;
#endif
using TapEmpire.Services;
using UnityEngine;

namespace TapEmpire.Experimental
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/v3/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        public AdsConfig Config;

        public bool EnableMetica = false;
#if TEL_METICA
        public MeticaUnitySdk MeticaPrefab;
#endif

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

        [Header("Banner / MREC positioning")]
        public AdPosition BannerPosition = AdPosition.BottomCenter;
        public BannerWidth BannerSize = BannerWidth.Full;
        public AdPosition MrecPosition = AdPosition.BottomCenter;

        [Header("Scene settings")]
        public List<SceneSettings> SceneSettings = new();

        [Header("Analytics")]
        public AdsAnalyticsSettings AdsAnalyticsSettings;

        [Header("Other")]
        public bool ShowApplovinOn2GB = false;
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

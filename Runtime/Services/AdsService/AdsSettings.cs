using System.Collections.Generic;
using Sirenix.OdinInspector;
using TapEmpireLibrary.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        [Header("On-Off")]
        public bool EnableUMP = true;
        public bool EnableATT = true;
        public bool EnableMetica = true;
        public bool EnableAppOpen = true;
        public bool EnableBanners = true;
        public bool DisableAdsForPayers = true;
        public bool ShouldWaitAppOpen = false;
        public float AppOpenWaitTime = 10.0f;

        public bool ShowApplovinOn2GB = false;
        public float InterstitialDelay = 30.0f;

        [Header("Interstitials by level")]
        public List<int> InterstitialAfterLevels = new();

        [Header("Interstitials by timer")]
        public int FromLevel = 0;
        public List<TimerData> TimerData = new();

        [Header("Scene settings")]
        public List<SceneSettings> SceneSettings = new();

        [Header("Analytics settings")]
        public List<RevenueLayer> RevenueLayers = new();
        public AdsAnalyticsSettings AdsAnalyticsSettings = null;

        [Button]
        public void Clear()
        {
            InterstitialAfterLevels.Clear();
        }

        [Button]
        public void AddLevelsBasedOnPattern(LevelsPattern pattern, int length)
        {
            var levelIndexes = pattern.GetLevels(length);
            InterstitialAfterLevels.AddRange(levelIndexes);
            InterstitialAfterLevels = RemoveDuplicatesAndSort(InterstitialAfterLevels);
        }

        private List<int> RemoveDuplicatesAndSort(List<int> list)
        {
            var distinctList = new HashSet<int>(list);
            var sortedList = new List<int>(distinctList);
            sortedList.Sort();
            return sortedList;
        }
    }

    [System.Serializable]
    public struct TimerData
    {
        public int FromLevel;
        public int Seconds;
    }

    [System.Serializable]
    public struct SceneSettings
    {
        public SceneName SceneName;
        public bool IsBannerEnabled;
    }

    [System.Serializable]
    public struct RevenueLayer
    {
        public string Name;
        public float Value;
    }
}

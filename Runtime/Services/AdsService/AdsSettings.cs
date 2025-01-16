using System.Collections.Generic;
using Sirenix.OdinInspector;
using TapEmpire.Services.AdsStrategy;
using TapEmpireLibrary.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        [Header("On-Off")]
        public bool EnableAppOpen = true;
        public bool ShouldWaitAppOpen = false;
        public float AppOpenWaitTime = 10.0f;

        public bool ShowApplovinOn2GB = false;
        public float InterstitialDelay = 30.0f;
        public AdsInterstitialStrategyType AdsInterstitialType;

        [Header("---List type---")]
        [Space(5)]
        [ShowIf("@this.AdsInterstitialType == AdsInterstitialStrategyType.List")]
        public List<int> InterstitialAfterLevels = new();
        
        [Header("---Session type---")]
        [ShowIf("@this.AdsInterstitialType == AdsInterstitialStrategyType.Sessions")]
        public AdsInterstitialSessionData AdsInterstitialSessionData;
        
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
}

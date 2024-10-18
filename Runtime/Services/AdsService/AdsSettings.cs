using System.Collections.Generic;
using System.Linq;
using RagDoll.Level;
using Sirenix.OdinInspector;
using TapEmpireLibrary.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        [Header("On-Off")]
        public bool EnableAppOpen = true;

        [Space(5)]
        public List<int> InterstitialAfterLevels = new();
        
        [Button]
        public void Clear()
        {
            InterstitialAfterLevels.Clear();
        }
        
        [Button]
        public void AddLevelsBasedOnPattern(LevelsPattern pattern, LevelsTable levelsTable)
        {
            var levelIndexes = pattern.GetLevels(levelsTable.UnsortedLevels.Length);
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

using System.Collections.Generic;
using System.Linq;
using RagDoll.Level;
using Sirenix.OdinInspector;
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

        public enum LevelPattern
        {
            Pattern_4_8_11_Plus3,
            Pattern_Each5
        }

        [Button]
        public void Clear()
        {
            InterstitialAfterLevels.Clear();
        }
        
        [Button]
        public void AddLevelsBasedOnPattern(LevelPattern pattern, LevelsTable levelsTable)
        {
            List<int> levelIndexes = new();

            switch (pattern)
            {
                case LevelPattern.Pattern_4_8_11_Plus3:
                    levelIndexes = GeneratePattern_4_8_11_Plus3(levelsTable.UnsortedLevels.Length);
                    break;
                case LevelPattern.Pattern_Each5:
                    levelIndexes = GeneratePattern_Each5(levelsTable.UnsortedLevels.Length);
                    break;
            }

            InterstitialAfterLevels.AddRange(levelIndexes);
            InterstitialAfterLevels = RemoveDuplicatesAndSort(InterstitialAfterLevels);
        }

        private List<int> GeneratePattern_4_8_11_Plus3(int totalLevels)
        {
            var indexes = new List<int> { 4, 8, 11 };
            var nextValue = 11;

            while (nextValue + 3 < totalLevels)
            {
                nextValue += 3;
                indexes.Add(nextValue);
            }

            return indexes.Where(index => index < totalLevels).ToList();
        }

        private List<int> GeneratePattern_Each5(int totalLevels)
        {
            var indexes = new List<int>();
            var nextValue = 5;

            while (nextValue < totalLevels)
            {
                indexes.Add(nextValue);
                nextValue += 5;
            }

            return indexes;
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

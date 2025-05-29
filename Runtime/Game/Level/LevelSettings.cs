using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TapEmpire.Level
{
    [CreateAssetMenu(menuName = "TapEmpire/LevelSettings", fileName = "_Level")]
    public class LevelSettings : ScriptableObject
    {
        [AssetsOnly]
        public AssetReference LevelViewPrefab = null;

        public string FullName => name;
        public string CustomName;
        public int IndexName;

        public void OnValidate()
        {
            IndexName = int.TryParse(name.Split('_')[0], out int levelNumber) ? levelNumber : -1;
            CustomName = name.RemoveFirstBlock('_');
        }

        // Deprecated
        public int GetLevelName()
        {
            string prefix = "_Level";
            if (name.EndsWith(prefix))
            {
                string numberString = name.Split('_')[0];
                if (int.TryParse(numberString, out int levelNumber))
                {
                    return levelNumber;
                }
            }
            return -1;
        }

        // Deprecated
        public string GetLevelShortName()
        {
            string prefix = "_Level";
            return name.EndsWith(prefix) ? name.RemoveLastOccurence(prefix) : name;
        }

        // Deprecated
        public string GetLevelFullName() => name;
    }
}
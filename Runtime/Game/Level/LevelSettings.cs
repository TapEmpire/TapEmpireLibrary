using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TapEmpire.Level
{
    [CreateAssetMenu(menuName = "TapEmpire/LevelSettings", fileName = "_Level")]
    public class LevelSettings : ScriptableObject
    {
        [AssetsOnly]
        public AssetReference LevelViewPrefab = null;

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

        public string GetLevelShortName()
        {
            string prefix = "_Level";
            return name.EndsWith(prefix) ? name.RemoveLastOccurence(prefix) : name;
        }

        public string GetLevelFullName() => name;

        // new

        public string FullName => name;
        public string CustomName => name.RemoveFirstBlock('_');
        public int IndexName => int.TryParse(name.Split('_')[0], out int levelNumber) ? levelNumber : -1;
    }
}
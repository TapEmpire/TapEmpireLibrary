using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class EditorCustomUtility
    {
        public static T LoadFirstAsset<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            if (guids.Length == 0) return null;
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Multiple {typeof(T).Name} assets found, using the first.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static List<T> LoadAllAssetsOfType<T>() where T : Object
        {
            var list = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) list.Add(asset);
            }
            return list;
        }
    }
}

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

        public static ScriptableObject LoadFirstAsset(string name)
        {
            var guids = AssetDatabase.FindAssets($"{name} t:ScriptableObject");

            if (guids.Length == 0) return null;
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Multiple {name} assets found, using the first.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }

        public static List<T> LoadAllAssetsOfType<T>(params string[] folders) where T : Object
        {
            var filter = $"t:{typeof(T).Name}";
            var guids = folders is { Length: > 0 }
                ? AssetDatabase.FindAssets(filter, folders)
                : AssetDatabase.FindAssets(filter);

            var list = new List<T>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) list.Add(asset);
            }
            return list;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BgTools.PlayerPrefsEditor;
using UnityEditor;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class PlayerPrefsExporter
    {
        private static int ERROR_VALUE_INT = int.MinValue;
        private static string ERROR_VALUE_STR = "<bgTool_error_24072017>";

        private static string CompanyName => Application.companyName;
        private static string ProductName => Application.productName;
        private static string RegistryPath => $@"Software\Unity\UnityEditor\{CompanyName}\{ProductName}";

        [MenuItem("Tools/BG Tools/Save")]
        private static void ExportPlayerPrefs()
        {
#if UNITY_EDITOR_WIN
            ExportWindows();
#elif UNITY_EDITOR_OSX
            ExportMac();
#endif
        }

#if UNITY_EDITOR_WIN
        private static void ExportWindows()
        {
            var storage = new WindowsPrefStorage(RegistryPath);
            var keys = storage.GetKeys();
            ExportByKeys(keys);
        }
#endif

#if UNITY_EDITOR_OSX
        private static void ExportMac()
        {
            var keys = GetMacPlayerPrefsKeys();
            ExportByKeys(keys);
        }
#endif

        private static void ExportByKeys(IEnumerable<string> keys)
        {
            var listDest = new List<PreferenceEntry>();

            foreach (var key in keys)
            {
                if (key.StartsWith("unity.")) continue;

                var entry = new PreferenceEntry { m_key = key };

                string s = PlayerPrefs.GetString(key, ERROR_VALUE_STR);
                if (s != ERROR_VALUE_STR)
                {
                    entry.m_strValue = s;
                    entry.m_typeSelection = PreferenceEntry.PrefTypes.String;
                    listDest.Add(entry);
                    continue;
                }

                float f = PlayerPrefs.GetFloat(key, float.NaN);
                if (!float.IsNaN(f))
                {
                    entry.m_floatValue = f;
                    entry.m_typeSelection = PreferenceEntry.PrefTypes.Float;
                    listDest.Add(entry);
                    continue;
                }

                int i = PlayerPrefs.GetInt(key, ERROR_VALUE_INT);
                if (i != ERROR_VALUE_INT)
                {
                    entry.m_intValue = i;
                    entry.m_typeSelection = PreferenceEntry.PrefTypes.Int;
                    listDest.Add(entry);
                }
            }

            SaveListAsJson(listDest);
        }

        private static void SaveListAsJson(List<PreferenceEntry> listDest)
        {
            var json = JsonUtility.ToJson(new Wrapper { entries = listDest }, true);
            var path = EditorUtility.SaveFilePanel("Save PlayerPrefs", "", "playerprefs.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, json);
            Debug.Log($"PlayerPrefs saved to: {path}");
        }

        [MenuItem("Tools/BG Tools/Load")]
        private static void LoadFromJsonAndApplyToPlayerPrefs()
        {
            var path = EditorUtility.OpenFilePanel("Load PlayerPrefs", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            var json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<Wrapper>(json);

            if (wrapper?.entries == null || wrapper.entries.Count == 0)
            {
                Debug.LogWarning("Invalid or empty PlayerPrefs file.");
                return;
            }

            PlayerPrefs.DeleteAll();

            foreach (var entry in wrapper.entries)
            {
                switch (entry.m_typeSelection)
                {
                    case PreferenceEntry.PrefTypes.String:
                        PlayerPrefs.SetString(entry.m_key, entry.m_strValue);
                        break;
                    case PreferenceEntry.PrefTypes.Int:
                        PlayerPrefs.SetInt(entry.m_key, entry.m_intValue);
                        break;
                    case PreferenceEntry.PrefTypes.Float:
                        PlayerPrefs.SetFloat(entry.m_key, entry.m_floatValue);
                        break;
                }
            }

            PlayerPrefs.Save();
            Debug.Log($"Loaded {wrapper.entries.Count} PlayerPrefs from {path}");
        }

#if UNITY_EDITOR_OSX
        /// <summary>
        /// Получение всех ключей PlayerPrefs в Unity Editor на macOS
        /// </summary>
        private static IEnumerable<string> GetMacPlayerPrefsKeys()
        {
            var keys = new List<string>();

            var editorPrefsType = typeof(EditorPrefs);
            var method = editorPrefsType.GetMethod(
                "GetString",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(string) },
                null
            );

            var keysField = editorPrefsType.GetField(
                "s_Keys",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            if (keysField?.GetValue(null) is IDictionary<string, object> dict)
            {
                keys.AddRange(dict.Keys);
            }

            return keys;
        }
#endif

        [Serializable]
        private class Wrapper
        {
            public List<PreferenceEntry> entries;
        }
    }
}

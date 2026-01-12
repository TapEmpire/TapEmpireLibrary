using System.Collections.Generic;
using System.IO;
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
            var storage = new WindowsPrefStorage(RegistryPath);
            var keys = storage.GetKeys();

            var listDest = new List<PreferenceEntry>();
            foreach (var key in keys)
            {
                if (key.StartsWith("unity.")) continue;
                
                var entry = new PreferenceEntry();
                entry.m_key = key;
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
                    continue;
                }
            }

            SaveListAsJson(listDest);
#endif
        }


        private static void SaveListAsJson(List<PreferenceEntry> listDest)
        {
            var json = JsonUtility.ToJson(new Wrapper {entries = listDest}, true);
            var path = EditorUtility.SaveFilePanel("Save list", "", "playerprefs.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, json);
            Debug.Log($"PreferenceEntry saved to: {path}");
        }

        [System.Serializable]
        private class Wrapper
        {
            public List<PreferenceEntry> entries;
        }

        [MenuItem("Tools/BG Tools/Load")]
        public static void LoadFromJsonAndApplyToPlayerPrefs()
        {
            var path = EditorUtility.OpenFilePanel("Load PlayerPrefs", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            var json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<Wrapper>(json);

            if (wrapper == null || wrapper.entries == null || wrapper.entries.Count == 0)
            {
                Debug.LogWarning("File does not contain valid data.");
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
            Debug.Log($"Loaded and applied {wrapper.entries.Count} records from {path}");
        }
    }
}
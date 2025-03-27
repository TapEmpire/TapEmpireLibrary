using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace TapEmpire.Utility.GoogleSheet
{
    [CreateAssetMenu(menuName = "TapEmpire/GoogleSheet/GoogleSheetData", fileName = "GoogleSheetData")]
    public class GoogleSheetData : ScriptableObject
    {
        [ShowInInspector]
        public static string SheetId = "1fAzhrnaDemi2lngCobeOdD-U0RbNoAczEpcjt_7qd38";
        public List<LevelGoogleData> List = new();
    }

    [Serializable]
    public class LevelGoogleData
    {
        public string LocalizationTableName;
        public string LevelId;
        
        [Button]
        public async void LoadFromGoogle()
        {
            var provider = new GoogleSheetsSettingsProvider();
            var localizationData = await provider.GetLocalization(GoogleSheetData.SheetId, LevelId);

            var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTableName);
            var dict = new Dictionary<string, StringTable>();
            foreach (var table in collection.StringTables)
            {
                table.Clear();
                dict.Add(table.name.Split('_').Last().Split('-')[0], table);
            }

            for (var i = 0; i < localizationData.Count; i++)
            {
                var container = localizationData[i];
                for (var j = 0; j < container.Entries.Count; j++)
                {
                    var containerEntry = container.Entries[j];
                    var table = dict[containerEntry.Locale];
                    if (containerEntry.Locale == "en") continue;

                    CreateOrUpdateEntry(table, containerEntry.LocalizedString, container.Key);
                }

                foreach (var table in dict.Values)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }
            }
        }

        private void CreateOrUpdateEntry(StringTable table, string localizedString, string entryName)
        {
            var entry = table.GetEntry(entryName);
            if (entry != null)
            {
                entry.Value = localizedString;
            }
            else
            {
                table.AddEntry(entryName, localizedString);
            }
        }
    }

    public class TranslationEntryContainer
    {
        public string Key;
        public List<TranslationEntry> Entries = new();
    }

    public class TranslationEntry
    {
        public string Locale;
        public string LocalizedString;
    }
}
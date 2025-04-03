using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace TapEmpire.Utility.GoogleSheet
{
    [CreateAssetMenu(menuName = "TapEmpire/GoogleSheet/GoogleSheetData", fileName = "GoogleSheetData")]
    public class GoogleSheetData : ScriptableObject
    {
        public TextAsset ServiceAccountKey;
        public string SpreadSheetId = string.Empty;
        public string TemplateSheetId = string.Empty;
        public List<LevelGoogleData> List = new();

        [SerializeReference] public List<ILocalizationConverter> Converters = new();

        [HideInInspector]
        public ConnectionData ConnectionData = null;

        private static GoogleSheetData _instance = null;

        public static GoogleSheetData Instance
        {
            get
            {
                if (_instance == null)
                {
                    var guids = AssetDatabase.FindAssets("t:GoogleSheetData");
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<GoogleSheetData>(path);
                }

                return _instance;
            }
        }

        [Button]
        private async void CheckAndCreateGoogleSheets()
        {
            var entries = List.Where(entry => string.IsNullOrEmpty(entry.TableId) || entry.TableId == "-1");
            foreach (var entry in entries)
            {
                await entry.CheckAndCreateGoogleSheetAsync();
            }
        }

        [Button]
        private async void LoadFromGoogle()
        {
            var entries = List.Where(entry => !string.IsNullOrEmpty(entry.TableId));
            foreach (var entry in entries)
            {
                await entry.LoadFromGoogleSheet();
            }
        }
    }

    [Serializable]
    public class LevelGoogleData
    {
        public string LocalizationTableName;
        public string TableId;

        [Button]
        public async void LoadFromGoogle()
        {
            await LoadFromGoogleSheet();
        }

        public async UniTask LoadFromGoogleSheet()
        {
            var googleSheetData = GoogleSheetData.Instance;
            var provider = new GoogleSheetsSettingsProvider();
            var localizationData = await provider.GetLocalization(googleSheetData.SpreadSheetId, TableId);
            googleSheetData.Converters.ForEach(converter => localizationData = converter.Deconvert(LocalizationTableName, localizationData));

            var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTableName);
            var dict = new Dictionary<string, StringTable>();
            foreach (var table in collection.StringTables)
            {
                dict.Add(table.name.Split('_').Last().Split('-')[0], table);
            }

            for (var i = 0; i < localizationData.Count; i++)
            {
                var container = localizationData[i];
                for (var j = 0; j < container.Entries.Count; j++)
                {
                    var containerEntry = container.Entries[j];
                    var table = dict[containerEntry.Locale];
                    // if (containerEntry.Locale == "en") continue;

                    CreateOrUpdateEntry(table, containerEntry.LocalizedString, container.Key);
                }

                foreach (var table in dict.Values)
                {
                    EditorUtility.SetDirty(table);
                    EditorUtility.SetDirty(table.SharedData);
                }
            }
        }

        [Button]
        private void CopyToClipboard()
        {
            GoogleSheetUtility.CopyToClipboard(LocalizationTableName);
        }

        [Button]
        private void CheckAndCreateGoogleSheet()
        {
            CheckAndCreateGoogleSheetAsync().Forget();
        }

        public async UniTask CheckAndCreateGoogleSheetAsync()
        {
            var tableId = await GoogleSheetUtility.CreateAndInitializeGoogleSheet(GoogleSheetData.Instance, LocalizationTableName);
            TableId = tableId.ToString();
            EditorUtility.SetDirty(GoogleSheetData.Instance);
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

    public class ConnectionData
    {
        public string ServiceKey;
        public string Token;
        public DateTime Expiration;
    }
}
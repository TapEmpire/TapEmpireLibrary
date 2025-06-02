using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TapEmpire.Build;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Utility.GoogleSheet
{
    [CreateAssetMenu(menuName = "TapEmpire/GoogleSheet/DefaultInitializer", fileName = "DefaultInitializer")]
    public class GoogleSheetDefaultInitializer : ScriptableObject
    {
        public string TableName = string.Empty;
        public string Path = string.Empty;
        public List<GoogleSheetDefaultEntry> Entries;

        [Button]
        private void GenerateLocalizationTable()
        {
            GoogleSheetUtility.GenerateLocalizationTable(Path, TableName, Entries);
        }

        [Button]
        private void ResetToDefaultPath()
        {
            Path = $"{ProjectPathSettings.Instance.DefaultGamePath}/Art/Localization";           
        }

        [Button]
        private void CopyToClipboardForGoogle()
        {
            GoogleSheetUtility.CopyToClipboard(Entries);
        }
    }

    [Serializable]
    public struct GoogleSheetDefaultEntry
    {
        public string Id;
        public string En;
    }
}
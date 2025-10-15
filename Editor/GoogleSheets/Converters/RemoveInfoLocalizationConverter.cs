using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Utility.GoogleSheet;
using UnityEngine;
using TapEmpire.Utility;
using System.Text;
using System.Text.RegularExpressions;

namespace WordGame.Editor
{
    [Serializable]
    public class RemoveInfoLocalizationConverter : ILocalizationConverter
    {
        public List<List<string>> Convert(string tableName, List<List<string>> data)
        {
            return data;
        }

        public List<TranslationEntryContainer> Deconvert(string tableName, List<TranslationEntryContainer> data, StringBuilder stringBuilder)
        {
            data.ForEach(container =>
                container.Entries.ForEach(entry => entry.LocalizedString = Regex.Replace(entry.LocalizedString, @"\s*\([^()]*\)\s*", "")));
            return data;
        }
    }
}

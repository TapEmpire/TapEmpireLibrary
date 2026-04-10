using System;
using System.Collections.Generic;
using TapEmpire.Utility.GoogleSheet;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TapEmpire.Editor
{
    [Serializable]
    public class TokenSubstitution
    {
        public string Token;
        public string Substitute;
    }

    [Serializable]
    public class RemoveInfoLocalizationConverter : ILocalizationConverter
    {
        [SerializeField] private List<TokenSubstitution> _substitutions = new();

        public List<List<string>> Convert(string tableName, List<List<string>> data)
        {
            return data;
        }

        public List<TranslationEntryContainer> Deconvert(string tableName, List<TranslationEntryContainer> data, StringBuilder stringBuilder)
        {
            data.ForEach(container =>
                container.Entries.ForEach(entry =>
                {
                    entry.LocalizedString = Regex.Replace(entry.LocalizedString, @"\s*\([^()]*\)\s*", " ");

                    foreach (var sub in _substitutions)
                    {
                        entry.LocalizedString = Regex.Replace(entry.LocalizedString, Regex.Escape(sub.Token), sub.Substitute);
                    }

                    entry.LocalizedString = entry.LocalizedString.Trim();
                }));
            return data;
        }
    }
}

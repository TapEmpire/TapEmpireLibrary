using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility.GoogleSheet
{
    [ExecuteInEditMode]
    public class GoogleSheetsSettingsProvider
    {
        private const char tableSeparator = '\t';

        private static class Selectors
        {
            public static T AddNewElement<T>(List<T> list) where T : new()
            {
                var element = new T();
                list.Add(element);
                return element;
            }
        }

        private class MultipleTableParseSettings
        {
            public int HeaderColumnIndex = 0;
            public string HeaderColumnName = "Name";

            public Action<IMultipleTablesParse, Dictionary<string, string>> SetHeader = null;
            public Action<IMultipleTablesParse, object> SetContents = null;
        }

        private const string _urlFormat = "https://docs.google.com/spreadsheets/d/{0}/export?gid={1}&exportFormat=tsv";

        public async Task<List<TranslationEntryContainer>> GetLocalization(string tableId, string levelId)
        {
            var url = string.Format(_urlFormat, tableId, levelId);
            var res = await SendGetRequest(url);

            var parsers = new Dictionary<string, Action<TranslationEntryContainer, string>>
            {
                ["key"] = (c, v) => c.Key = v,
                ["en"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "en", LocalizedString = v }),
                ["es"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "es", LocalizedString = v }),
                ["pt"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "pt", LocalizedString = v }),
                ["it"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "it", LocalizedString = v }),
                ["de"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "de", LocalizedString = v }),
                ["zh"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "zh", LocalizedString = v }),
                ["ja"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "ja", LocalizedString = v }),
                ["ko"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "ko", LocalizedString = v }),
                ["in"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "in", LocalizedString = v }),
                ["fr"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "fr", LocalizedString = v }),
                ["vi"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "vi", LocalizedString = v }),
                ["ms"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "ms", LocalizedString = v }),
                ["id"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "id", LocalizedString = v }),
                ["hi"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "hi", LocalizedString = v }),
                ["ru"] = (c, v) => c.Entries.Add(new TranslationEntry() { Locale = "ru", LocalizedString = v }),
            };

            var tables = ParsePlainData(res.downloadHandler.text, parsers, Selectors.AddNewElement, ParseAsTable);
            tables = tables.Where(table => !string.IsNullOrEmpty(table.Key)).ToList();
            Debug.Log($"[SETTINGS] Remote table {levelId} data set");

            return tables;
        }

        private UniTask<UnityWebRequest> SendGetRequest(string url)
        {
            return UnityWebRequest.Get(url).SendWebRequest().ToUniTask();
        }

        private List<T> ParsePlainData<T>(string table, IReadOnlyDictionary<string, Action<T, string>> parsers, Func<List<T>, T> selector, Action<IReadOnlyDictionary<string, Action<T, string>>, T, string[], string[]> tableParseMethod) where T : new()
        {
            var lookup = new Dictionary<string, string>();

            table = table.Replace("\r", "");
            var rows = table.Split('\n');

            var headerItems = rows[0].Split(tableSeparator);
            foreach (var headerItem in headerItems)
                lookup[headerItem] = null;

            var result = new List<T>();
            foreach (var row in rows.Skip(1))
            {
                var rowItems = row.Split(tableSeparator);
                for (var i = 0; i < headerItems.Length; i++)
                    lookup[headerItems[i]] = rowItems[i];

                var element = selector(result);

                tableParseMethod(parsers, element, rowItems, headerItems);
            }

            return result;
        }

        private List<T1> ParseMultiLineTableData<T, T1>(MultipleTableParseSettings parseSettings, string table, IReadOnlyDictionary<string, Action<T, string>> parsers, Func<List<T>, T> selector, Action<IReadOnlyDictionary<string, Action<T, string>>, T, string[], string[]> tableParseMethod) where T1 : IMultipleTablesParse, new() where T : new()
        {
            var rows = table.Split('\n');
            var headerItems = rows[0].Split(tableSeparator);
            headerItems[headerItems.Length - 1] = headerItems[headerItems.Length - 1].Replace("\r", "");
            var tables = SplitToTables(rows, tableSeparator);

            var result = new List<T1>();

            for (int i = 0; i < tables.Count; i++)
            {
                var current = tables[i];
                var lookup = new Dictionary<string, string>();

                var unlockDataEntity = new T1();

                var tableResult = new List<T>();
                for (var index = 0; index < current.Count; index++)
                {
                    var row = current[index];
                    var rowItems = row.Split(tableSeparator);
                    for (var j = 0; j < headerItems.Length; j++)
                    {
                        lookup[headerItems[j]] = rowItems[j];
                    }

                    var element = selector(tableResult);

                    tableParseMethod(parsers, element, rowItems, headerItems);

                    if (index == parseSettings.HeaderColumnIndex)
                    {
                        parseSettings.SetHeader(unlockDataEntity, lookup);
                    }
                }

                parseSettings.SetContents(unlockDataEntity, tableResult);

                if (unlockDataEntity is T1 dataEntity)
                {
                    result.Add(dataEntity);
                }
            }

            return result;
        }

        private List<List<string>> SplitToTables(string[] rows, char tableSeparator)
        {
            var tables = new List<List<string>>();
            var singleTable = new List<string>();
            var tableCounter = 0;

            for (int i = 1; i < rows.Length; i++)
            {
                var row = rows[i];
                if (!row.StartsWith(tableSeparator))
                {
                    if (tableCounter > 0 || singleTable.Count > 0)
                    {
                        tables.Add(singleTable);
                        tableCounter++;
                    }

                    singleTable = new List<string>();
                }

                row = row.Replace("\r", "");
                singleTable.Add(row);

                if (i >= rows.Length - 1)
                {
                    tables.Add(singleTable);
                }
            }

            return tables;
        }

        private void ParseAsTable<T>(IReadOnlyDictionary<string, Action<T, string>> parsers, T element, string[] items, string[] headers = null) where T : new()
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (!parsers.ContainsKey(headers[i]))
                    continue;
                parsers[headers[i]](element, items[i]);
            }
        }

        private void ParseByRows<T>(IReadOnlyDictionary<string, Action<T, string>> parsers, T element, string[] items, string[] headers = null) where T : new()
        {
            if (!parsers.ContainsKey(items[0]))
                return;

            parsers[items[0]](element, items[1]);
        }
    }
}
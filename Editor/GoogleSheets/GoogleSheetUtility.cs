using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Networking;

namespace TapEmpire.Utility.GoogleSheet
{
    public static class GoogleSheetUtility
    {
        public static void GenerateLocalizationTable(string path, string tableName, List<GoogleSheetDefaultEntry> entries)
        {
            var fullPath = $"{path}/{tableName}";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);

            if (stringTableCollection == null)
            {
                stringTableCollection = LocalizationEditorSettings.CreateStringTableCollection(
                    tableName, fullPath, LocalizationSettings.AvailableLocales.Locales);
            }

            var keys = entries.Select(entry => entry.Id);

            stringTableCollection.StringTables.ForEach(table => table.Clear());
            stringTableCollection.SharedData.Entries
                .Where(entry => !keys.Contains(entry.Key))
                .Select(entry => entry.Key)
                .ToList()
                .ForEach(key => stringTableCollection.SharedData.RemoveKey(key));

            var table = stringTableCollection.GetTable("en-US") as StringTable;
            entries.ForEach(entry => table.AddEntry(entry.Id, entry.En));

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
        }

        public static void CopyToClipboard(List<GoogleSheetDefaultEntry> entries)
        {
            StringBuilder sb = new StringBuilder();

            entries.ForEach(entry => sb.AppendLine($"{entry.Id}\t{entry.En}"));

            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("Data copied to clipboard - ready to paste into Google Sheets");
        }

        // public static async UniTaskVoid UpdateGoogleSheet(string jsonData, string sheetName, string spreadsheetId, string apiKey)
        // {
        //     string range = $"{sheetName}!A1";
        //     string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?valueInputOption=USER_ENTERED&key={apiKey}";

        //     using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        //     {
        //         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        //         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //         request.downloadHandler = new DownloadHandlerBuffer();
        //         request.SetRequestHeader("Content-Type", "application/json");

        //         await request.SendWebRequest();

        //         if (request.result == UnityWebRequest.Result.Success)
        //         {
        //             UnityEngine.Debug.Log("Google Sheet updated successfully!");
        //         }
        //         else
        //         {
        //             UnityEngine.Debug.LogError($"Error updating sheet: {request.error}");
        //             UnityEngine.Debug.LogError($"Response: {request.downloadHandler.text}");
        //         }
        //     }
        // }
        // private static void Test()
        // {
        //     var sheetName = "UI2";
        //     var spreadsheetId = "";
        //     var apiKey = "";

        //     List<List<object>> values = new List<List<object>>
        //     {
        //         new List<object> { "Row1Col1", "Row1Col2", "Row1Col3" },
        //         new List<object> { "Row2Col1", "Row2Col2", "Row2Col3" },
        //         new List<object> { DateTime.Now.ToString(), UnityEngine.Random.Range(1, 100), "Unity Data" }
        //     };

        //     var json = FormatJsonForSheets(values, sheetName);
        //     UpdateGoogleSheet(json, sheetName, spreadsheetId, apiKey).Forget();
        // }

        // private static string FormatJsonForSheets(List<List<object>> values, string sheetName)
        // {
        //     var valueRange = new Dictionary<string, object>()
        //     {
        //         { "range", $"{sheetName}!A1" },
        //         { "majorDimension", "ROWS" },
        //         { "values", values }
        //     };

        //     return JsonConvert.SerializeObject(valueRange);
        //     // return JsonUtility.ToJson(valueRange);
        // }
    }
}
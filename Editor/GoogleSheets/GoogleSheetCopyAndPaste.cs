using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TapEmpire.Utility.GoogleSheet
{
    public class GoogleSheetCopyAndPaste
    {
        public static async UniTask<int> DuplicateAndPopulateSheet(GoogleSheetData googleSheetData,
            string sheetName, List<List<string>> data)
        {
            if (googleSheetData.ConnectionData == null || googleSheetData.ConnectionData.Expiration <= DateTime.Now)
            {
                Debug.Log("Getting new access token...");
                googleSheetData.ConnectionData = await GoogleSheetUtility.GetAccessTokenAsync(googleSheetData.ServiceAccountKey.text);

                if (googleSheetData.ConnectionData == null)
                {
                    return -1;
                }
            }

            Debug.Log("Duplicating sheet...");
            string duplicateResponse = await DuplicateSheetAsync(googleSheetData, sheetName);

            if (string.IsNullOrEmpty(duplicateResponse))
            {
                Debug.LogError("Failed to duplicate sheet");
                return -1;
            }

            int newSheetId = ParseSheetIdFromResponse(duplicateResponse);
            if (newSheetId < 0)
            {
                Debug.LogError("Failed to parse new sheet ID");
                return -1;
            }

            Debug.Log($"Sheet duplicated successfully with ID: {newSheetId}");

            await PopulateSheetAsync(googleSheetData, sheetName, data);
            Debug.Log("Process completed successfully!");

            return newSheetId;
        }

        private static UniTask<string> DuplicateSheetAsync(GoogleSheetData googleSheetData, string newSheetName)
        {
            var spreadSheetData = googleSheetData.SpreadSheets.GetSpreadSheetData(newSheetName);
            string spreadsheetId = spreadSheetData.Id;
            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}:batchUpdate";

            string jsonBody = $@"{{
                ""requests"": [
                    {{
                        ""duplicateSheet"": {{
                            ""sourceSheetId"": {spreadSheetData.TemplateSheetId},
                            ""insertSheetIndex"": 9999,
                            ""newSheetName"": ""{newSheetName}""
                        }}
                    }}
                ]
            }}";

            return GoogleSheetUtility.SendRequest(url, "POST", jsonBody, googleSheetData.ConnectionData.Token);
        }

        public static async UniTask DeleteSheetAsync(GoogleSheetData googleSheetData, string tableId, string tableName)
        {
            if (googleSheetData.ConnectionData == null || googleSheetData.ConnectionData.Expiration <= DateTime.Now)
            {
                Debug.Log("Getting new access token...");
                googleSheetData.ConnectionData = await GoogleSheetUtility.GetAccessTokenAsync(googleSheetData.ServiceAccountKey.text);

                if (googleSheetData.ConnectionData == null)
                {
                    Debug.LogWarning("Failed to create connection data");
                    return;
                }
            }

            var spreadSheetData = googleSheetData.SpreadSheets.GetSpreadSheetData(tableName);
            string spreadsheetId = spreadSheetData.Id;
            
            Debug.Log($@"Removing from sheet {spreadsheetId} with id {tableId} ({tableName})");

            var response = await DeleteSheetAsyncInternal(googleSheetData, spreadsheetId, tableId);

            Debug.Log($@"Removing sheet response {response}");
        }

        private static UniTask<string> DeleteSheetAsyncInternal(GoogleSheetData googleSheetData, string spreadsheetId, string tableId)
        {

            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}:batchUpdate";

            string jsonBody = $@"{{
                ""requests"": [
                    {{
                        ""deleteSheet"": {{
                            ""sheetId"": {tableId}
                        }}
                    }}
                ]
            }}";

            return GoogleSheetUtility.SendRequest(url, "POST", jsonBody, googleSheetData.ConnectionData.Token);
        }

        private static UniTask<string> PopulateSheetAsync(GoogleSheetData googleSheetData, string sheetName, List<List<string>> data)
        {
            string range = $"{sheetName}!A2:{(char) ('A' + data[0].Count - 1)}{data.Count + 1}";
            string spreadsheetId = googleSheetData.SpreadSheets.GetSpreadSheet(sheetName);
            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?valueInputOption=USER_ENTERED";

            var requestBody = new
            {
                values = data
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            return GoogleSheetUtility.SendRequest(url, "PUT", jsonBody, googleSheetData.ConnectionData.Token);
        }

        private static int ParseSheetIdFromResponse(string jsonResponse)
        {
            try
            {
                JObject jObject = JObject.Parse(jsonResponse);
                return (int) jObject["replies"][0]["duplicateSheet"]["properties"]["sheetId"];
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing sheet ID: {ex.Message}");
                return -1;
            }
        }
    }
}
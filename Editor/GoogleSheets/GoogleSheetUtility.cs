using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Networking;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;

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

            if (stringTableCollection != null)
            {
                // var keys = entries.Select(entry => entry.Id);

                // stringTableCollection.StringTables.ForEach(table => table.Clear());
                // stringTableCollection.SharedData.Entries
                //     .Where(entry => !keys.Contains(entry.Key))
                //     .Select(entry => entry.Key)
                //     .ToList()
                //     .ForEach(key => stringTableCollection.SharedData.RemoveKey(key));
            }

            if (stringTableCollection == null)
            {
                stringTableCollection = LocalizationEditorSettings.CreateStringTableCollection(tableName, fullPath);
            }

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

        public static void CopyToClipboard(string tableName)
        {
            var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            var table = stringTableCollection.GetTable("en-US") as StringTable;
            var sharedTable = stringTableCollection.SharedData;

            StringBuilder sb = new StringBuilder();

            sharedTable.Entries.ForEach(entry => sb.AppendLine($"{entry.Key}\t{table.GetEntry(entry.Id).Value}"));

            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("Data copied to clipboard - ready to paste into Google Sheets");
        }

        public static List<List<string>> GetLocalizationTableData(string tableName)
        {
            var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            var table = stringTableCollection.GetTable("en-US") as StringTable;
            var sharedTable = stringTableCollection.SharedData;

            return sharedTable.Entries
                .Select(entry => new List<string>() { entry.Key, table.GetEntry(entry.Id).Value })
                .ToList();
        }

        public static UniTask<int> CreateAndInitializeGoogleSheet(GoogleSheetData googleSheetData, string tableName)
        {
            var data = GetLocalizationTableData(tableName);
            return GoogleSheetCopyAndPaste.DuplicateAndPopulateSheet(googleSheetData, tableName, data);
        }

        public static async UniTask<string> SendRequest(string url, string method, string data, string token)
        {
            try
            {
                using (UnityWebRequest request = new UnityWebRequest(url, method))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();

                    request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.SetRequestHeader("Content-Type", "application/json");
                        request.SetRequestHeader("Authorization", "Bearer " + token);
                    }

                    var response = await request.SendWebRequest().ToUniTask();

                    if (response.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Successful request");
                        return request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError("Response error: " + response.error);
                        Debug.LogError("Response: " + request.downloadHandler.text);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in UnityWebRequest: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public static async UniTask<ConnectionData> GetAccessTokenAsync(string serviceAccountJson)
        {
            string url = "https://oauth2.googleapis.com/token";
            ServiceAccountData saData = JsonConvert.DeserializeObject<ServiceAccountData>(serviceAccountJson);

            string jwt = CreateJwt(saData);
            string formData = $"grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={jwt}";

            var response = await SendRequest(url, "POST", formData, null);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }

            TokenResponse tokenResponse = JsonUtility.FromJson<TokenResponse>(response);

            return new ConnectionData()
            {
                ServiceKey = serviceAccountJson,
                Token = tokenResponse.access_token,
                Expiration = DateTime.Now.AddSeconds(tokenResponse.expires_in - 300)
            };
        }

        private static string Base64UrlEncode(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            string base64 = Convert.ToBase64String(inputBytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static string CreateJwt(ServiceAccountData saData)
        {
            string header = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long expirationTime = currentTime + 3600; // 1 hour

            string claimSet = $@"{{
                    ""iss"":""{saData.client_email}"",
                    ""scope"":""https://www.googleapis.com/auth/spreadsheets"",
                    ""aud"":""https://oauth2.googleapis.com/token"",
                    ""exp"":{expirationTime},
                    ""iat"":{currentTime}
                }}";

            // Encode the JWT components
            string encodedHeader = Base64UrlEncode(header);
            string encodedClaimSet = Base64UrlEncode(claimSet);

            // Create signature
            string signatureInput = encodedHeader + "." + encodedClaimSet;
            string signature = SignWithRSA_BouncyCastle(signatureInput, saData.private_key);

            // Assemble the JWT
            return signatureInput + "." + signature;
        }

        public static string SignWithRSA_BouncyCastle(string input, string privateKey)
        {
            try
            {
                // Clean up the private key - remove headers and line breaks
                privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", "")
                                      .Replace("-----END PRIVATE KEY-----", "")
                                      .Replace("\n", "")
                                      .Replace("\r", "");

                // Decode the private key from Base64
                byte[] privateKeyBytes = Convert.FromBase64String(privateKey);

                // Load the private key using BouncyCastle
                AsymmetricKeyParameter privKey;

                try
                {
                    // Parse as PKCS#8 key
                    privKey = PrivateKeyFactory.CreateKey(privateKeyBytes);
                }
                catch (Exception)
                {
                    // If PKCS#8 fails, try as a regular private key
                    privKey = PrivateKeyFactory.CreateKey(privateKeyBytes);
                }

                // Create a signer for SHA-256 with RSA
                ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");

                // Initialize the signer with the private key for signing
                signer.Init(true, privKey);

                // Convert input to bytes and add to the signer
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                signer.BlockUpdate(inputBytes, 0, inputBytes.Length);

                // Generate the signature
                byte[] signatureBytes = signer.GenerateSignature();

                // Convert signature to Base64 URL encoded format
                string signature = Convert.ToBase64String(signatureBytes);
                return signature.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
            catch (Exception ex)
            {
                Debug.LogError($"BouncyCastle signing error: {ex.Message}\n{ex.StackTrace}");
                return string.Empty;
            }
        }
    }
}
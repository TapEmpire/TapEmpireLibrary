using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class IapPurchaseData
    {
        public string Key { get; set; }
        public DateTime PurchaseTime { get; set; }
    }

    public static class IAPDataKeys
    {
        public const string RestoredIapKey = "RestoredIapKey";
    }

    public class IapStorage
    {
        public IapPurchaseData Load(IapSettings iapSettings)
        {
            var storageData = PlayerPrefs.GetString(iapSettings.Key, String.Empty);
            if (storageData.Equals(String.Empty))
            {
                return JsonConvert.DeserializeObject<IapPurchaseData>(storageData);
            }

            return null;
        }

        public List<IapPurchaseData> Load(IReadOnlyCollection<IapSettings> iapSettings)
        {
            var outData = new List<IapPurchaseData>(iapSettings.Count);
            
            foreach (var iap in iapSettings)
            {
                var iapData = Load(iap);
                if (iapData != null)
                {
                    outData.Add(iapData);
                }
            }

            return outData;
        }
    }
}
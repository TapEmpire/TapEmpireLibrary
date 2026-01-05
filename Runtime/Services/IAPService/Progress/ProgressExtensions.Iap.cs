using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        private const string PurchasesTag = "Purchases";
        private const string PayerTag = "Payer";
        private const string TopSpendTag = "TopSpend";
        private const string TotalSpendTag = "TotalSpend";

        public static int GetPurchases(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(PurchasesTag, out var value) ? value : default;
        }

        public static bool HasPurchases(this IProgressService self)
        {
            return self.GetPurchases() > 0;
        }

        public static void AddPurchase(this IProgressService self)
        {
            self.UpdateInt(PurchasesTag);
        }

        public static void SavePurchaseIds(this IProgressService self, HashSet<string> value)
        {
            var save = JsonConvert.SerializeObject(value);
            self.StringValuesDictionary.SetValue(IapShowProgressKey, save);
        }

        public static HashSet<string> GetPurchaseIds(this IProgressService self)
        {
            var hashSet = new HashSet<string>();
            if (self.StringValuesDictionary.TryGetValue(IapShowProgressKey, out var value, canUseDefault: false))
            {
                hashSet = JsonConvert.DeserializeObject<HashSet<string>>(value) ?? new();
            }

            return hashSet;
        }

        public static void SetIsPayer(this IProgressService self, bool value)
        {
            self.BoolValuesDictionary.SetValue(PayerTag, value);
        }

        public static bool GetIsPayer(this IProgressService self)
        {
            return self.BoolValuesDictionary.TryGetValue(PayerTag, out var value) ? value : default;
        }

        public static void ClearIsPayer(this IProgressService self)
        {
            self.BoolValuesDictionary.SetValue(PayerTag, default);
        }

        public static SpendData GetTotalSpend(this IProgressService self) => self.GetSerializableObject<SpendData>(TotalSpendTag, true);
        public static void SetTotalSpend(this IProgressService self, SpendData value) => self.SetSerializableObject(TotalSpendTag, value);
        public static void ClearTotalSpend(this IProgressService self) => self.StringValuesDictionary.DeleteKey(TotalSpendTag);

        public static SpendData GetTopSpend(this IProgressService self) => self.GetSerializableObject<SpendData>(TopSpendTag, true);
        public static void SetTopSpend(this IProgressService self, SpendData value) => self.SetSerializableObject(TopSpendTag, value);
        public static void ClearTopSpend(this IProgressService self) => self.StringValuesDictionary.DeleteKey(TopSpendTag);
    }

    [Serializable]
    public class SpendData
    {
        public decimal Value;
        public string IsoCode;
    }
}
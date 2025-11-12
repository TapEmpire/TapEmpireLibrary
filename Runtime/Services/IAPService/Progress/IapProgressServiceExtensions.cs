using System.Collections.Generic;
using Newtonsoft.Json;

namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public const string PurchasesTag = "Purchases";

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
    }
}
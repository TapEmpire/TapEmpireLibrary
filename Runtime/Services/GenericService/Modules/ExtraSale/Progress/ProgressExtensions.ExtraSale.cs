using TapEmpire.Services;
using UnityEngine;

namespace TapEmpire.Modules
{
    public static partial class ProgressServiceExtensions
    {
        private static string TopCoinsKey = "TopCoins";

        public static int GetTopCoinsPurchased(this IProgressService self) => self.GetInt(TopCoinsKey);
        public static void SetTopCoinsPurchased(this IProgressService self, int value) => self.SetInt(TopCoinsKey, value);
        public static void ClearTopCoinsPurchased(this IProgressService self) => self.IntValuesDictionary.DeleteKey(TopCoinsKey);
    }
}

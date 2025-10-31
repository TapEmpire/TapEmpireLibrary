using System;

namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        private const string AdRevenueKey = "AdRevenue";
        private const string AdRevenueBatchedKey = "AdRevenueBatched";
        private const string AdOnceKey = "AdOnce";
        private const float MillionFloat = 1000000.0f;
        private const double MillionDouble = 1000000.0;

        private static int GetIntAdRevenue(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(AdRevenueKey, out var value, canUseDefault: false) ? value : 0;
        }

        public static float GetAdRevenue(this IProgressService self)
        {
            return self.GetIntAdRevenue() / MillionFloat;
        }

        public static float UpdateAdRevenue(this IProgressService self, double revenue)
        {
            var total = self.GetIntAdRevenue();
            total += (int)Math.Floor(revenue * MillionFloat);
            self.IntValuesDictionary.SetValue(AdRevenueKey, total);
            return total / MillionFloat;
        }

        private static int GetIntAdRevenueBatched(this IProgressService self, string prefix, string postfix = "")
        {
            return self.IntValuesDictionary.TryGetValue(GetAdRevenueBatchedKey(postfix), out var value, canUseDefault: false) ? value : 0;
        }

        public static double GetAdRevenueBatched(this IProgressService self, string postfix = "")
        {
            return self.GetIntAdRevenueBatched(postfix) / MillionDouble;
        }

        public static void SetAdRevenueBatched(this IProgressService self, double revenue, string postfix = "")
        {
            var value = (int)Math.Floor(revenue * MillionDouble);
            self.IntValuesDictionary.SetValue(GetAdRevenueBatchedKey(postfix), value);
        }

        public static void ClearAdRevenueBatched(this IProgressService self, string postfix = "")
        {
            self.IntValuesDictionary.SetValue(GetAdRevenueBatchedKey(postfix), 0);
        }

        public static bool GetOnceBatched(this IProgressService self, string postfix = "")
        {
            return self.BoolValuesDictionary.TryGetValue(GetAdOnceKey(postfix), out var value) ? value : default;
        }

        public static void SetOnceBatched(this IProgressService self, string postfix = "")
        {
            self.BoolValuesDictionary.SetValue(GetAdOnceKey(postfix), true);
        }

        private static string GetAdRevenueBatchedKey(string postfix) => $"{AdRevenueBatchedKey}{postfix}";
        private static string GetAdOnceKey(string postfix) => $"{AdOnceKey}{postfix}";
    }
}
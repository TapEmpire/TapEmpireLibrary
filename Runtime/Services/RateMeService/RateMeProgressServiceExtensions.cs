namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public const string RateMeTag = "RateMe";

        public static void SetRateMe(this IProgressService self, bool value)
        {
            self.BoolValuesDictionary.SetValue(RateMeTag, value);
        }

        public static bool GetRateMe(this IProgressService self)
        {
            return self.BoolValuesDictionary.TryGetValue(RateMeTag, out var value) ? value : default;
        }

        /*public static void SetRateMeLevelIndex(this IProgressService self, int value)
        {
            var key = $"{RateMeTag}_{value}";
            self.IntValuesDictionary.SetValue(key, value);
        }
        
        public static bool TryRateMeLevelIndex(this IProgressService self, int levelIndex)
        {
            var key = $"{RateMeTag}_{levelIndex}";
            return self.IntValuesDictionary.TryGetValue(key, out var value, canUseDefault: false);
        }*/
    }
}
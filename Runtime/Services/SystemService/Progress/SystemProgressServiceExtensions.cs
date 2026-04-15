namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public const string PlayOffline = "PlayOffline";
        public const string PlayOfflineNoAds = "PlayOfflineNoAds";

        public static void SetPlayOffline(this IProgressService self, bool value)
        {
            self.BoolValuesDictionary.SetValue(PlayOffline, value);
        }

        public static bool GetPlayOffline(this IProgressService self, bool defaultValue)
        {
            return self.BoolValuesDictionary.TryGetValue(PlayOffline, out var value) ? value : defaultValue;
        }

        public static void ClearPlayOffline(this IProgressService self)
        {
            self.BoolValuesDictionary.SetValue(PlayOffline, default);
        }

        public static void SetPlayOfflineNoAds(this IProgressService self, bool value)
        {
            self.BoolValuesDictionary.SetValue(PlayOfflineNoAds, value);
        }

        public static bool GetPlayOfflineNoAds(this IProgressService self, bool defaultValue)
        {
            return self.BoolValuesDictionary.TryGetValue(PlayOfflineNoAds, out var value) ? value : defaultValue;
        }
    }
}
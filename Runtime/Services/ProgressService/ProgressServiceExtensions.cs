namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        #region Main

        public static void SetIntProp(this IProgressService self, ProgressIntProp prop, int value)
        {
            self.IntValuesDictionary.SetValue(prop.ToString(), value);
        }

        public static void SetBoolProp(this IProgressService self, ProgressBoolProp prop, bool value)
        {
            self.BoolValuesDictionary.SetValue(prop.ToString(), value);
        }

        public static bool TryGetIntProp(this IProgressService self, ProgressIntProp prop, out int value, bool canUseCached = true, bool canUseDefault = true, bool setNotCached = true)
        {
            return self.IntValuesDictionary.TryGetValue(prop.ToString(), out value, canUseCached, canUseDefault, setNotCached);
        }

        public static bool TryGetBoolProp(this IProgressService self, ProgressBoolProp prop, out bool value, bool canUseCached = true, bool canUseDefault = true, bool setNotCached = true)
        {
            return self.BoolValuesDictionary.TryGetValue(prop.ToString(), out value, canUseCached, canUseDefault, setNotCached);
        }

        public static int GetIntProp(this IProgressService self, ProgressIntProp prop)
        {
            return self.IntValuesDictionary.TryGetValue(prop.ToString(), out var value) ? value : default;
        }

        public static int UpdateIntProp(this IProgressService self, ProgressIntProp prop)
        {
            var key = $"{prop}";
            var currentValue = self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
            var nextValue = currentValue + 1;
            self.IntValuesDictionary.SetValue(key, nextValue);
            return nextValue;
        }

        #endregion

        private const string RemoteConfigNameKey = "RemoteConfigNameKey";

        #region generic

        public static string GetRemoteConfigName(this IProgressService self)
        {
            return self.StringValuesDictionary.TryGetValue(RemoteConfigNameKey, out var value) ? value : default;
        }

        public static void SetRemoteConfigName(this IProgressService self, string configName)
        {
            self.StringValuesDictionary.SetValue(RemoteConfigNameKey, configName);
        }

        #endregion

        #region analytics

        public static int GetSessionsStarted(this IProgressService self) => self.GetIntProp(ProgressIntProp.SessionsStarted);

        public static int UpdateSessionsStarted(this IProgressService self) => self.UpdateIntProp(ProgressIntProp.SessionsStarted);

        #endregion analytics

        #region level

        public static int GetLevelProgress(this IProgressService self)
        {
            var key = $"{ProgressIntProp.CompletedLevelCount}";
            return self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static void SetLevelProgress(this IProgressService self, int value)
        {
            var key = $"{ProgressIntProp.CompletedLevelCount}";
            self.IntValuesDictionary.SetValue(key, value);
        }

        public static int GetCyclesProgress(this IProgressService self)
        {
            var key = $"{ProgressIntProp.CyclesCompleted}";
            return self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static int UpdateCyclesProgress(this IProgressService self)
        {
            return self.UpdateIntProp(ProgressIntProp.CyclesCompleted);
        }

        public static int GetAdsWatchedProgress(this IProgressService self)
        {
            var key = $"{ProgressIntProp.TotalAdsWatched}";
            return self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static int UpdateAdsWatchedProgress(this IProgressService self)
        {
            return self.UpdateIntProp(ProgressIntProp.TotalAdsWatched);
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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
            return UpdateInt(self, $"{prop}");
        }

        public static int UpdateInt(this IProgressService self, string key)
        {
            var currentValue = self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
            var nextValue = currentValue + 1;
            self.IntValuesDictionary.SetValue(key, nextValue);
            return nextValue;
        }

        public static int GetInt(this IProgressService self, string key)
        {
            return self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static void SetInt(this IProgressService self, string key, int value)
        {
            self.IntValuesDictionary.SetValue(key, value);
        }

        public static bool GetBool(this IProgressService self, string key)
        {
            return self.BoolValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static bool GetBool(this IProgressService self, ProgressBoolProp prop)
        {
            return self.TryGetBoolProp(prop, out var value) ? value : default;
        }

        public static void SetBool(this IProgressService self, string key, bool value)
        {
            self.BoolValuesDictionary.SetValue(key, value);
        }

        public static string GetString(this IProgressService self, string key)
        {
            return self.StringValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static void SetString(this IProgressService self, string key, string value)
        {
            self.StringValuesDictionary.SetValue(key, value);
        }

        public static void SetCurrentTimeStamp(this IProgressService self, string key)
        {
            self.StringValuesDictionary.SetValue(key, System.DateTime.UtcNow.ToString());
        }

        public static void SetTimeStamp(this IProgressService self, string key, System.DateTime dateTime)
        {
            self.StringValuesDictionary.SetValue(key, dateTime.ToString());
        }

        public static void CleanTimeStamp(this IProgressService self, string key)
        {
            self.StringValuesDictionary.DeleteKey(key);
        }

        public static System.DateTime GetTimeStamp(this IProgressService self, string key)
        {
            return self.GetTimeStampDefault(key, System.DateTime.UtcNow);
        }

        public static System.DateTime GetTimeStampDefault(this IProgressService self, string key, System.DateTime defaultValue = default)
        {
            var dateString = self.StringValuesDictionary.TryGetValue(key, out var value) ? value : default;
            return System.DateTime.TryParse(dateString, out var date) ? date : defaultValue;
        }

        public static T GetSerializableObject<T>(this IProgressService self, string key, bool createNew = false) where T : new()
        {
            if (self.StringValuesDictionary.TryGetValue(key, out var value, canUseDefault: false))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            return createNew ? new() : default(T);
        }

        public static void SetSerializableObject<T>(this IProgressService self, string key, T data)
        {
            var saveJson = JsonConvert.SerializeObject(data);
            self.StringValuesDictionary.SetValue(key, saveJson);
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

        private static string VersionKey = "VersionKey";

        public static string GetVersion(this IProgressService self)
        {
            return self.StringValuesDictionary.TryGetValue(VersionKey, out var value, canUseDefault: false) ? value : string.Empty;
        }

        public static void SetVersion(this IProgressService self)
        {
            self.StringValuesDictionary.SetValue(VersionKey, Application.version);
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

        public static int GetTotalLevels(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(nameof(ProgressIntProp.TotalLevels), out var value) ? value : default;
        }

        public static void SetTotalLevels(this IProgressService self, int value)
        {
            self.IntValuesDictionary.SetValue(nameof(ProgressIntProp.TotalLevels), value);
        }
        
        public static string GetVisualProgress(this IProgressService self)
        {
            var key = $"{ProgressIntProp.VisualProgress}";
            return self.StringValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static void SetVisualProgress(this IProgressService self, string value)
        {
            var key = $"{ProgressIntProp.VisualProgress}";
            self.StringValuesDictionary.SetValue(key, value);
        }

        public static int GetCyclesProgress(this IProgressService self)
        {
            var key = $"{ProgressIntProp.CyclesCompleted}";
            return self.IntValuesDictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static void ClearCyclesProgress(this IProgressService self)
        {
            self.SetIntProp(ProgressIntProp.CyclesCompleted, 0);
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

        #region Iaps

        public static string IapShowProgressKey = "IapShowProgressData";

        public static List<int> GetIapShowProgress(this IProgressService self)
        {
            var list = new List<int>();
            if (self.StringValuesDictionary.TryGetValue(IapShowProgressKey, out var value, canUseDefault: false))
            {
                list = JsonConvert.DeserializeObject<List<int>>(value) ?? new();
            }

            return list;
        }

        public static void SetIapShowProgress(this IProgressService self, List<int> value)
        {
            var serializedObject = JsonConvert.SerializeObject(value);

            self.StringValuesDictionary.SetValue(IapShowProgressKey, serializedObject);
        }

        public static void CleanIapShowProgress(this IProgressService self)
        {
            self.StringValuesDictionary.SetValue(IapShowProgressKey, "");
        }
        
        #endregion
    }
}
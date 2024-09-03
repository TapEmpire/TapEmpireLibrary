using UnityEngine;

namespace TapEmpire.Utility
{
    // todo по-хорошему все это перенести в ProgressService, чтобы можно было подписываться на значения + хранить промежуточные значения
    public static class PlayerPrefsUtility
    {
        public static string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);

        public static void SetIntFast(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public static void SetFloatFast(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public static void SetStringFast(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
        
        private static void SetBoolFast(string key, bool value) => SetIntFast(key, value ? 1 : 0);

        private static bool GetBool(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;

        private static System.DateTime GetDateFromString(string dateString)
        {
            return System.DateTime.TryParse(dateString, out var date) ? date : System.DateTime.UtcNow;
        }

        #region Common

        private const string ShouldLoadConfigKey = "ShouldLoadConfigKey";

        public static void SetShouldLoadConfig(bool value) => SetBoolFast(ShouldLoadConfigKey, value);

        public static bool GetShouldLoadConfig() => GetBool(ShouldLoadConfigKey, true);

        #endregion

        #region Ads
        public const string RestartAmountKey = "RestartAmountKey";

        public static int UpdateRestartAmount()
        {
            var amount = PlayerPrefs.GetInt(RestartAmountKey, 0);
            SetIntFast(RestartAmountKey, amount + 1);
            return amount + 1;
        }

        #endregion

        #region Audio
        
        public const string MusicSettingsKey = "MusicSettingsKey";
        
        public const string SoundSettingsKey = "SoundSettingsKey";

        public static float GetSoundSettings(float defaultValue) => PlayerPrefs.GetFloat(SoundSettingsKey, defaultValue);

        public static void SetSoundSettings(float value) => SetFloatFast(SoundSettingsKey, value);

        public static float GetMusicSettings(float defaultValue) => PlayerPrefs.GetFloat(MusicSettingsKey, defaultValue);

        public static void SetMusicSettings(float value) => SetFloatFast(MusicSettingsKey, value);

        #endregion

        #region Haptic
        
        private const string HapticSettingsKey = "HapticSettingsKey";

        public static bool GetHapticSettings(bool defaultValue) => GetBool(HapticSettingsKey, defaultValue);

        public static void SetHapticSettings(bool value) => SetBoolFast(HapticSettingsKey, value);

        #endregion

        #region Rate me
        
        private const string RateMeKey = "RateMeKey";

        public static bool GetRateMe(bool defaultValue) => GetBool(RateMeKey, defaultValue);

        public static void SetRateMe(bool value) => SetBoolFast(RateMeKey, value);
        
        #endregion
        
        #region Session + launch

        private const string FirstLaunchDateKey = "FirstLaunchDateKey";
        private const string SessionEndKey = "SessionEndKey";

        public static (bool, System.DateTime) GetFirstLaunch()
        {
            var firstLaunchDate = PlayerPrefs.GetString(FirstLaunchDateKey, "");
            var isFirstLaunch = string.IsNullOrEmpty(firstLaunchDate);

            if (isFirstLaunch)
            {
                firstLaunchDate = System.DateTime.UtcNow.ToString();
                PlayerPrefs.SetString(FirstLaunchDateKey, firstLaunchDate);
            }

            return (isFirstLaunch, GetDateFromString(firstLaunchDate));
        }

        public static void SetSessionStart()
        {
            // var sessionEnd = GetDate(SessionEndKey);
            // UpdateTimeSpanAtSessionStart(XKey, XSpanKey, sessionEnd);
        }

        public static void SetSessionEnd()
        {
            // SetStringFast(SessionEndKey, System.DateTime.UtcNow.ToString());
        }

        private static void UpdateTimeSpanAtSessionStart(string stampKey, string timeSpanKey, System.DateTime sessionEnd)
        {
            var timeStampString = PlayerPrefs.GetString(stampKey, "");
            var timeSpan = PlayerPrefs.GetFloat(timeSpanKey, 0.0f);

            if (!string.IsNullOrEmpty(timeStampString))
            {
                var timeStamp = GetDateFromString(timeStampString);
                if (sessionEnd > timeStamp)
                {
                    timeSpan += (float)(sessionEnd - timeStamp).TotalSeconds;
                }
            }

            PlayerPrefs.SetFloat(timeSpanKey, timeSpan);
            PlayerPrefs.SetString(stampKey, System.DateTime.UtcNow.ToString());
            PlayerPrefs.Save();
        }

        #endregion Session + launch
    }
}
namespace TapEmpire.Services
{
    public static partial class AnalyticsEvents
    {
        public const string SessionStart = "SESSION_START";
        public const string SessionEnd = "SESSION_END";
        public const string LaunchFirstTime = "LAUNCH_FIRST_TIME";
        public const string AdsButtonClicked = "ADS_BUTTON_CLICKED";
        public const string AdsInterstitialCheck = "INTERSTITIAL_CHECK";
        public const string AdsStarted = "ADS_STARTED";
        public const string AdsWatched = "ADS_WATCHED";
        public const string AdsPayed = "ADS_PAYED";
    }

    public static partial class AnalyticsParameters
    {
        public const string InstallYear = "InstallYear";
        public const string InstallDate = "InstallDate";
        public const string DaysAfterInstall = "DaysAfterInstall";
        public const string AdjustAttribution = "Attribution";
        public const string RemoteConfig = "RemoteConfig";
        public const string AdsWatched = "Ads_watched";
    }
}

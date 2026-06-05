namespace TapEmpire.Services
{
    public static class AdsAnalyticsEvents
    {
        public const string AdsButtonClicked = "ADS_BUTTON_CLICKED";
        public const string AdsInterstitialCheck = "ADS_INTERSTITIAL_CHECK";
        public const string AdsStarted = "ADS_STARTED";
        public const string AdsWatched = "ADS_WATCHED";
        public const string AdsPayed = "ADS_PAYED";
        public const string IsMeticaEnabled = "Metica_Enabled";
    }

    public static class AdsAnalyticsParameters
    {
        public const string AdsWatched = "Ads_watched";
    }

    public static class AdsAnalyticsStrings
    {
        public const string AdsPlacements = "AdsPlacements";
    }
}

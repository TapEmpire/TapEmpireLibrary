namespace TapEmpire.Services
{
    public static class AdsAnalyticsEvents
    {
        public const string AdsButtonClicked = "ADS_BUTTON_CLICKED";
        public const string AdsInterstitialCheck = "INTERSTITIAL_CHECK";
        public const string AdsStarted = "ADS_STARTED";
        public const string AdsWatched = "ADS_WATCHED";
        public const string AdsPayed = "ADS_PAYED";
    }

    public static class AdsAnalyticsParameters
    {
        public const string AdsWatched = "Ads_watched";
    }
}

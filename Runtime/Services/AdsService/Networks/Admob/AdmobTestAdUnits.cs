namespace TapEmpire.Services
{
    // Google's sample AdMob ad unit IDs. They always serve test ads on any device without
    // test-device registration, so providers use them in test mode instead of the real units.
    // https://developers.google.com/admob/unity/test-ads
    public static class AdmobTestAdUnits
    {
#if UNITY_IOS
        public const string Banner = "ca-app-pub-3940256099942544/2934735716";
        public const string Interstitial = "ca-app-pub-3940256099942544/4411468910";
        public const string Rewarded = "ca-app-pub-3940256099942544/1712485313";
#else
        public const string Banner = "ca-app-pub-3940256099942544/6300978111";
        public const string Interstitial = "ca-app-pub-3940256099942544/1033173712";
        public const string Rewarded = "ca-app-pub-3940256099942544/5224354917";
#endif
        public const string Mrec = Banner; // MREC reuses the banner sample unit
    }
}

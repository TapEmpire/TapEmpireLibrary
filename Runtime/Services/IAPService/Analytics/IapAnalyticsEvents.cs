namespace TapEmpire.Services
{
    public static class IapAnalyticsEvents
    {
        public const string IapPurchased = "PURCHASE_COMPLETED";
        public const string IapError = "PURCHASE_ERROR";
        public const string IapRestored = "PURCHASE_RESTORED";
    }

    public static class IapAnalyticsStrings
    {
        public const string AdsPlacements = "IapPacements";
    }
}
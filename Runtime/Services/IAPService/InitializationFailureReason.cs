namespace TapEmpire.Services
{
    public enum InitializationFailureReasonV4
    {
        PurchasingUnavailable,
        NoProductsAvailable,
        AppNotKnown
    }

    public enum InitializationFailureReason
    {
        StoreDisconnect,
        PurchasesFetchFailed,
        ProductsFetchFailed,
    }
}
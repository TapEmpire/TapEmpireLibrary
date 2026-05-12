using System;
using System.Collections.Generic;
using R3;
using TapEmpire.Services;

namespace TapEmpire.Experimental
{
    public interface IAttributionService : IService
    {
        ReadOnlyReactiveProperty<string> CampaignName { get; }

        void TrackEvent(string eventToken, IReadOnlyDictionary<string, string> callbackParameters = null);

        void TrackRevenue(
            string eventToken,
            double revenue,
            string isoCurrency,
            string productId,
            IReadOnlyDictionary<string, string> callbackParameters = null);

        void VerifyAndroidPurchase(string productId, string purchaseToken, Action<bool> callback);

        void VerifyApplePurchase(string transactionId, string productId, Action<bool> callback);
    }
}

#if TEL_META
using System.Collections.Generic;
using R3;
using TapEmpire.Services;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    internal class FacebookAdsSignalsModule : AdsSignalsModuleBase
    {
        public FacebookAdsSignalsModule(IAdsService adsService, IProgressService progressService, IIapService iapService)
            : base(adsService, progressService)
        {
            iapService.OnPurchaseSuccessDetailed.Subscribe(OnPurchaseDetailed).AddTo(_disposables);
        }

        protected override BatchedData InitializeBatchedData() =>
            new()
            {
                BatchType = _analyticsSettings.BatchTypeMeta,
                Threshold = _analyticsSettings.ThresholdMeta,
                Postfix = "Meta",
            };

        protected override LayeredData InitializeLayeredData() =>
            new()
            {
                RevenueLayers = _analyticsSettings.MetaRevenueLayers,
                Postfix = "Meta",
            };

        protected override void LogBatched(double revenue)
        {
            Facebook.Unity.FB.LogAppEvent("ad_revenue_batched", valueToSum: (float)revenue,
                parameters: new Dictionary<string, object>
                {
                    { "fb_currency", "USD" }
                });
        }

        protected override void LogLayered(string name, double revenue)
        {
            Facebook.Unity.FB.LogAppEvent(name, valueToSum: (float)revenue,
                parameters: new Dictionary<string, object>
                {
                    { "fb_currency", "USD" }
                });
        }

        private void OnPurchaseDetailed((Product product, string productId, string placement) data)
        {
            var price = (double)data.product.metadata.localizedPrice;
            var isoCode = data.product.metadata.isoCurrencyCode;

            if (isoCode == "USD")
            {
                if (_analyticsSettings.AddMetaIapBatched) ProcessBatched(price);
                if (_analyticsSettings.AddMetaIapLayered) ProcessLayered(price);
            }
        }
    }
}
#endif

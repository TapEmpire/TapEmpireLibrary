#if TEL_META
using System;
using System.Collections.Generic;
using R3;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    // Base Meta logging: ad impressions + IAP purchases. Created by FacebookService after FB.Init,
    // so FB is guaranteed initialized. Campaign-optimization revenue logging lives in FacebookAdsSignalsModule.
    internal class FacebookAnalyticsModule : IDisposable
    {
        private readonly AdsAnalyticsSettings _analyticsSettings;
        private readonly CompositeDisposable _disposables = new();

        public FacebookAnalyticsModule(IAdsService adsService, IIapService iapService)
        {
            _analyticsSettings = adsService.Settings.AdsAnalyticsSettings;

            adsService.OnImpressionUnsafe.Subscribe(OnImpression).AddTo(_disposables);
            iapService.OnPurchaseSuccessDetailed.Subscribe(OnPurchaseDetailed).AddTo(_disposables);
        }

        public void Dispose() => _disposables.Dispose();

        private void OnImpression(AdImpressionData data)
        {
            var platform = data.Mediation == AdNetwork.Admob ? "Admob" : "AppLovin";
            var source = data.Mediation == AdNetwork.Admob ? "Simple Admob" : data.Network;

            var parameters = new Dictionary<string, object>
            {
                { "ad_platform", platform },
                { "ad_source", source },
                { "ad_format", data.Format.ToString() },
                { "ad_placement_id", data.Placement },
                { "fb_currency", data.Currency },
            };

            if (!string.IsNullOrEmpty(data.Precision)) parameters["precision"] = data.Precision;

            Facebook.Unity.FB.LogAppEvent("ad_impression", valueToSum: (float)data.Revenue, parameters: parameters);
        }

        private void OnPurchaseDetailed((Product product, string productId, string placement) data)
        {
            if (!_analyticsSettings.EnableMetaPurchases) return;

            Facebook.Unity.FB.LogPurchase(data.product.metadata.localizedPrice, data.product.metadata.isoCurrencyCode,
                new Dictionary<string, object>
                {
                    { "fb_content_type", "product" },
                    { "fb_content_id", data.product.definition.id },
                    { "fb_order_id", data.product.transactionID ?? string.Empty }
                });
        }
    }
}
#endif

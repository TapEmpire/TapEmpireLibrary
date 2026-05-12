using System.Collections.Generic;
using Firebase.Analytics;
using Io.AppMetrica;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Experimental;
using TapEmpire.Modules;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace TapEmpire.Services
{
    public class IapAnalyticsModule : IServiceModule
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IAttributionService _attributionService;
        private readonly IIapService _iapService;
        private readonly IUIService _uiService;
        private readonly IProgressService _progressService;

        private AdsSettings _adsSettings;
        private CompositeDisposable _disposables = new();

        public IapAnalyticsModule(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _attributionService = diContainer.Resolve<IAttributionService>();
            _iapService = diContainer.Resolve<IIapService>();
            _uiService = diContainer.Resolve<IUIService>();
            _adsSettings = diContainer.Resolve<IAdsService>().Settings;

            _iapService.OnPurchaseSuccessDetailed.Subscribe(OnPurchaseSuccessDetailed).AddTo(_disposables);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed).AddTo(_disposables);
            _iapService.OnPurchaseRestored.Subscribe(OnPurchaseRestored).AddTo(_disposables);

            _uiService.OnBeforeOpenView += UiService_OnBeforeOpenView;
        }

        public void Dispose()
        {
            _uiService.OnBeforeOpenView -= UiService_OnBeforeOpenView;
            _disposables.Dispose();
        }

        private void OnPurchaseSuccessDetailed((Product product, string productKey, string placement) data)
        {
            var iapId = data.product.definition.id;
            var offer = _iapService.GetOfferInfoByStoreId(iapId);
            if (offer == null)
            {
                Debug.LogError($"cant find pack with id: {iapId}, stop sending analytics");
                return;
            }

            var levelsCompleted = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapPurchased, new Dictionary<string, object>()
            {
                { "purchase_id", new JObject(new JProperty(iapId, data.placement))},
                { "level", levelsCompleted }
            });

            var price = data.product.metadata.localizedPrice;
            var isoCode = data.product.metadata.isoCurrencyCode;

            SaveSpend(price, isoCode);

            var revenue = new Revenue((long)(price * 1_000_000m), isoCode);
            AppMetrica.ReportRevenue(revenue);

            _attributionService.TrackRevenue(_iapService.AdjustPurchaseToken, (double)price, isoCode, iapId);

            FirebaseAnalytics.LogEvent(IapAnalyticsEvents.IapPurchased, new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterValue, (double)price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, isoCode),
            });

#if TEL_META
            if (_adsSettings.AdsAnalyticsSettings.EnableMeta && _adsSettings.AdsAnalyticsSettings.EnableMetaPurchases)
            {
                Facebook.Unity.FB.LogPurchase(price, isoCode, new Dictionary<string, object>
                {
                    { "fb_content_type", "product" },
                    { "fb_content_id", iapId },
                    { "fb_order_id", data.product.transactionID ?? string.Empty }
                });
            }
#endif

            _analyticsService.LogEvent(IapAnalyticsStrings.AdsPlacements, new Dictionary<string, object>()
            {
                { iapId, "Purchased"}
            });

            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", "iap_purchased" },
                { "level", levelsCompleted },
                { "iap_status", "Success" },
                { "iap_product_id", iapId },
                // { "iap_order_id", product.transactionID },
                { "iap_price", price },
                { "iap_currency", isoCode }
            });
        }

        private void OnPurchaseFailed(PurchaseFailArgs args)
        {
            var levelsCompleted = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapError, new Dictionary<string, object>()
            {
                { "purchase_id", args.IapId },
                { "level", levelsCompleted },
                { "reason", args.Reason.ToString() },
            });

            var product = _iapService.GetProductInfoByStoreId(args.IapId);

            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", "iap_purchased" },
                { "level", levelsCompleted },
                { "iap_status", args.Reason.ToString() },
                { "iap_product_id", args.IapId },
                // { "iap_order_id", product.transactionID },
                { "iap_price", product?.metadata.localizedPrice },
                { "iap_currency", product?.metadata.isoCurrencyCode }
            });
        }

        private void OnPurchaseRestored(string iapId)
        {
            var levelsCompleted = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapRestored, new Dictionary<string, object>()
            {
                { "purchase_id", iapId },
                { "level", levelsCompleted }
            });
        }

        private void UiService_OnBeforeOpenView(IUIViewModel model)
        {
            switch (model)
            {
                case NoAdsPopupViewModel:
                    OnOpenNoAds(model as NoAdsPopupViewModel);
                    break;
            }
        }

        private void OnOpenNoAds(NoAdsPopupViewModel model)
        {
            _analyticsService.LogEvent(IapAnalyticsStrings.AdsPlacements, new Dictionary<string, object>()
            {
                { NoAdsPopupViewModel.IapKey, new JObject(new JProperty("Shown", model.Placement)) }
            });
        }

        private void SaveSpend(decimal price, string isoCode)
        {
            var totalSpend = _progressService.GetTotalSpend();
            if (totalSpend.IsoCode != isoCode)
            {
                totalSpend.IsoCode = isoCode;
                totalSpend.Value = 0;
            }
            totalSpend.Value += price;
            _progressService.SetTotalSpend(totalSpend);

            var topSpend = _progressService.GetTopSpend();
            if (topSpend.Value < price || topSpend.IsoCode != isoCode)
            {
                topSpend.Value = price;
                topSpend.IsoCode = isoCode;
                _progressService.SetTopSpend(topSpend);
            }
        }
    }
}
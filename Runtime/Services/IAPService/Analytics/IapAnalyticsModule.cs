using System.Collections.Generic;
using AdjustSdk;
using Firebase.Analytics;
using Io.AppMetrica;
using Newtonsoft.Json.Linq;
using R3;
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
        private readonly IIapService _iapService;
        private readonly IUIService _uiService;
        private readonly IProgressService _progressService;

        private AdsSettings _adsSettings;
        private CompositeDisposable _disposables = new();

        public IapAnalyticsModule(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
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

        private void OnPurchaseSuccessDetailed(Product product)
        {
            var iapId = product.definition.id;
            var offer = _iapService.GetOfferInfoByStoreId(iapId);
            if (offer == null)
            {
                Debug.LogError($"cant find pack with id: {iapId}, stop sending analytics");
                return;
            }

            var levelsCompleted = _progressService.GetVisualProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapPurchased, new Dictionary<string, object>()
            {
                { "purchase_id", iapId },
                { "level", levelsCompleted }
            });

            var price = product.metadata.localizedPrice;
            var isoCode = product.metadata.isoCurrencyCode;

            var revenue = new Revenue((long)(price * 1_000_000m), isoCode);
            AppMetrica.ReportRevenue(revenue);

            AdjustEvent adjustEvent = new AdjustEvent(_iapService.AdjustPurchaseToken);
            adjustEvent.SetRevenue((double)price, isoCode);
            adjustEvent.ProductId = iapId;
            // SetupVerificationData(adjustEvent, product);
            Adjust.TrackEvent(adjustEvent);

            FirebaseAnalytics.LogEvent(IapAnalyticsEvents.IapPurchased, new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterValue, (double)price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, isoCode),
            });

#if TEL_META
            if (_adsSettings.EnableMeta && _adsSettings.AdsAnalyticsSettings.EnableMetaPurchases)
            {
                Facebook.Unity.FB.LogPurchase(price, isoCode, new Dictionary<string, object>
                {
                    { "fb_content_type", "product" },
                    { "fb_content_id", iapId },
                    { "fb_order_id", product.transactionID }
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
                { "level", levelsCompleted }
            });

            var product = _iapService.GetProductInfoByStoreId(args.IapId);

            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", "iap_purchased" },
                { "level", levelsCompleted },
                { "iap_status", args.Reason.ToString() },
                { "iap_product_id", args.IapId },
                // { "iap_order_id", product.transactionID },
                { "iap_price", product.metadata.localizedPrice },
                { "iap_currency", product.metadata.localizedPriceString }
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

        private void SetupVerificationData(AdjustEvent adjustEvent, Product product)
        {
#if UNITY_EDITOR || IGNORE_VERIFICATION
            return;
#elif UNITY_ANDROID
            var unityReceipt = JsonUtility.FromJson<UnityReceipt>(product.receipt);
            var googleReceiptJson = JsonUtility.FromJson<GooglePlayReceiptJson>(unityReceipt.Payload);
            var googleReceipt = JsonUtility.FromJson<GooglePlayReceiptFixed>(googleReceiptJson.json);
            adjustEvent.PurchaseToken = googleReceipt.purchaseToken;
#elif UNITY_IOS
            adjustEvent.TransactionId = product.transactionID;
#endif
        }
    }
}
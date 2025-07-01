using System.Collections.Generic;
using com.adjust.sdk;
using Firebase.Analytics;
using Io.AppMetrica;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

namespace TapEmpire.Services
{
    public class IapAnalyticsModule
    {
        private readonly DiContainer _diContainer;
        private readonly IAnalyticsService _analyticsService;
        private readonly IIapService _iapService;
        private readonly IUIService _uiService;

        public IapAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
            _iapService = _diContainer.Resolve<IIapService>();
            _uiService = _diContainer.Resolve<IUIService>();
        }

        public void Initialize()
        {
            _iapService.OnPurchaseSuccessDetailed.Subscribe(OnPurchaseSuccessDetailed);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed);
            _iapService.OnPurchaseRestored.Subscribe(OnPurchaseRestored);

            _uiService.OnBeforeOpenView += UiService_OnBeforeOpenView;
        }

        public void Release()
        {
            _uiService.OnBeforeOpenView -= UiService_OnBeforeOpenView;
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
            
            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapPurchased, new Dictionary<string, object>()
            {
                { "purchase_id", iapId },
                { "level", levelsCompleted }
            });

            var price = product.metadata.localizedPrice;
            var isoCode = product.metadata.isoCurrencyCode;
            
            var revenue = new Revenue((long)price, isoCode);
            AppMetrica.ReportRevenue(revenue);
            
            AdjustEvent adjustEvent = new AdjustEvent(_iapService.AdjustPurchaseToken);
            adjustEvent.setRevenue((double)price, isoCode);
            adjustEvent.setProductId(iapId);
            adjustEvent.setPurchaseToken(product.transactionID);
            Adjust.trackEvent(adjustEvent);

            FirebaseAnalytics.LogEvent(IapAnalyticsEvents.IapPurchased, new Parameter[]
            {
                new("value", price.ToString()),
                new("currency", isoCode),
            });

            _analyticsService.LogEvent(IapAnalyticsStrings.AdsPlacements, new Dictionary<string, object>()
            {
                { iapId, "Purchased"}
            });
        }
        
        private void OnPurchaseFailed(PurchaseFailArgs args)
        {
            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapError, new Dictionary<string, object>()
            {
                { "purchase_id", args.IapId },
                { "level", levelsCompleted }
            });
        }

        private void OnPurchaseRestored(string iapId)
        {
            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
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
    }
}
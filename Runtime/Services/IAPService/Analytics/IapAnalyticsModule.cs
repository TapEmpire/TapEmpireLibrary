using System.Collections.Generic;
using com.adjust.sdk;
using Firebase.Analytics;
using Io.AppMetrica;
using R3;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    public class IapAnalyticsModule
    {
        private readonly DiContainer _diContainer;
        private readonly IAnalyticsService _analyticsService;
        private readonly IIapService _iapService;

        public IapAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
            _iapService = _diContainer.Resolve<IIapService>();
        }

        public void Initialize()
        {
            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed);
            _iapService.OnPurchaseRestored.Subscribe(OnPurchaseRestored);
        }

        private void OnPurchaseSuccess(string iapId)
        {
            var offer = _iapService.GetOfferInfo(iapId);
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
            
            var revenue = new Revenue((long)offer.Price, "USD");
            AppMetrica.ReportRevenue(revenue);
            
            var purchaseEventToken = "iap_purchase";
            AdjustEvent adjustEvent = new AdjustEvent(purchaseEventToken);
            adjustEvent.setRevenue(offer.Price, "USD");
            adjustEvent.setPurchaseToken(purchaseEventToken);
            Adjust.trackEvent(adjustEvent);

            FirebaseAnalytics.LogEvent(IapAnalyticsEvents.IapPurchased, new Parameter[]
            {
                new("value", offer.Price),
                new("currency", "USD"),
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
    }
}
using System.Collections.Generic;
using com.adjust.sdk;
using Firebase.Analytics;
using Io.AppMetrica;
using R3;
using UnityEngine.Purchasing;
using Zenject;

namespace TapEmpire.Services
{
    public class IapAnalyticsModule
    {
        private readonly DiContainer _diContainer;
        private readonly IAnalyticsService _analyticsService;

        public IapAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
        }

        public void Initialize()
        {
            var iapService = _diContainer.Resolve<IIapService>();
            iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess);
            iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed);
            iapService.OnPurchaseRestored.Subscribe(OnPurchaseRestored);
        }

        private void OnPurchaseSuccess(Product product)
        {
            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            _analyticsService.LogEvent(IapAnalyticsEvents.IapPurchased, new Dictionary<string, object>()
            {
                { "purchase_id", product.definition.id },
                { "level", levelsCompleted }
            });
            
            var revenue = new Revenue((long)product.metadata.localizedPrice, "USD");
            AppMetrica.ReportRevenue(revenue);
            
            var purchaseEventToken = "iap_purchase";
            AdjustEvent adjustEvent = new AdjustEvent(purchaseEventToken);
            adjustEvent.setRevenue((double)product.metadata.localizedPrice, "USD");
            adjustEvent.setPurchaseToken(purchaseEventToken);
            Adjust.trackEvent(adjustEvent);

            FirebaseAnalytics.LogEvent(IapAnalyticsEvents.IapPurchased, new Parameter[]
            {
                new("value", (long)product.metadata.localizedPrice),
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
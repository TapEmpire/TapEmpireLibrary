using System;
using System.Collections.Generic;
using R3;
using TapEmpire.Services.Analytics;
using TapEmpire.Services.Shop;
using Zenject;

namespace TapEmpire.Services.Offer
{
    public class ShopAnalyticsModule : IDisposable
    {
        private readonly IAnalyticsService _analyticsService;

        private CompositeDisposable _disposables = new();

        public ShopAnalyticsModule(DiContainer diContainer)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();

            var shopService = diContainer.Resolve<IShopService>();
            shopService.OnShopShown.Subscribe(OnShopShown).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnShopShown(string placement)
        {
            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>()
            {
                { "Shop", placement }
            });
        }
    }
}
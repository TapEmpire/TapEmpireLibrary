using TapEmpire.Modules;
using Zenject;
using R3;

namespace TapEmpire.Services
{
    public class IapExtraModule : IServiceModule
    {
        private IAdsService _adsService;
        private CompositeDisposable _disposables = new();

        public IapExtraModule(DiContainer diContainer)
        {
            var iapService = diContainer.Resolve<IIapService>();
            _adsService = diContainer.Resolve<IAdsService>();

            if (iapService.Settings.DisableAdsForPayers && !_adsService.AdsDisabled)
            {
                iapService.OnPurchaseSuccess.Subscribe(OnPurchase).AddTo(_disposables);
                iapService.OnPurchaseRestored.Subscribe(OnPurchase).AddTo(_disposables);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnPurchase(string purchaseId)
        {
            _adsService.DisableAds(true);
            _disposables.Dispose();
        }
    }
}
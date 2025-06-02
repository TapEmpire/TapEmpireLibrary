using System;
using System.Threading;
using R3;
using TapEmpire.Services;
using TapEmpire.UI;
using UnityEngine;

namespace TapEmpire.UI
{
    public class NoAdsPopupViewModel : IUIViewModel
    {
        public IIapService IapService => _iapService;
        public IUIService UiService => _uiService;

        private readonly IUIService _uiService;
        private readonly IIapService _iapService;

        public static readonly string IapKey = "no_ads_default";
        public string Placement { get; private set; } = String.Empty;

        private CompositeDisposable _disposables = new();

        public NoAdsPopupViewModel(IUIService uiService, IIapService iapService, string placement)
        {
            _uiService = uiService;
            _iapService = iapService;
            Placement = placement;
        }

        public void StartPurchase(string key = null)
        {
            Unsubscribe();
            _disposables = new CompositeDisposable();

            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed).AddTo(_disposables);
            _iapService.BuyProduct(key ?? IapKey);
        }

        public void Close()
        {
            _uiService.CloseViewAsync(this, CancellationToken.None);
        }

        public string GetPrice(string key = null)
        {
            var product = _iapService.GetProductInfo(key ?? IapKey);
            return product.metadata.localizedPriceString;
        }

        private void OnPurchaseSuccess(string productId)
        {
            _uiService.CloseViewAsync(this, CancellationToken.None);
            Unsubscribe();
        }

        private void OnPurchaseFailed(PurchaseFailArgs args)
        {
            Unsubscribe();
            Debug.Log($"OnPurchaseFailed but nothing to do, id: {args.IapId} reason: {args.Reason}");
        }

        private void Unsubscribe()
        {
            _disposables.Dispose();
        }
    }
}
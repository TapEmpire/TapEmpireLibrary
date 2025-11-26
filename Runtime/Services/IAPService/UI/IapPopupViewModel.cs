using System;
using R3;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    public class IapPopupViewModel : IUIViewModel, IInjectable, IDisposable
    {
        public IIapService IapService => _iapService;
        public IUIService UiService => _uiService;

        private IUIService _uiService;
        private IIapService _iapService;

        protected CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IUIService uiService, IIapService iapService)
        {
            _uiService = uiService;
            _iapService = iapService;
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void StartPurchase(string key)
        {
            Unsubscribe();
            _disposables = new CompositeDisposable();

            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed).AddTo(_disposables);
            _iapService.BuyProduct(key);
        }

        public void Close()
        {
            _uiService.CloseViewAsync(this, default);
        }

        public string GetPrice(string key)
        {
            var product = _iapService.GetProductInfo(key);
            return product.metadata.localizedPriceString;
        }

        private void OnPurchaseSuccess(string productId)
        {
            _uiService.CloseViewAsync(this, default);
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
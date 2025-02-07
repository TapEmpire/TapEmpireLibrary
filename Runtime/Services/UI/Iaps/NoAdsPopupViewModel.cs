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
        private readonly IUIService _uiService;
        private readonly IIapService _iapService;

        public static readonly string IapKey = "no_ads_default";
        public string Placement { get; private set; } = String.Empty;
        
        public NoAdsPopupViewModel(IUIService uiService, IIapService iapService, string placement)
        {
            _uiService = uiService;
            _iapService = iapService;
            Placement = placement;
        }

        public void StartPurchase()
        {
            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess);
            _iapService.OnPurchaseFailed.Subscribe(OnPurchaseFailed);
            _iapService.BuyProduct(IapKey);
        }
        
        public void Close()
        {
            _uiService.CloseViewAsync(this, CancellationToken.None);
        }
        
        public string GetPrice()
        {
            var product = _iapService.GetProductInfo(IapKey);
            return product.metadata.localizedPriceString;
        }

        private void OnPurchaseSuccess(string productId)
        {
            _uiService.CloseViewAsync(this, CancellationToken.None);
        }

        private void OnPurchaseFailed(PurchaseFailArgs args)
        {
            Debug.Log($"OnPurchaseFailed but nothing to do, id: {args.IapId} reason: {args.Reason}");
        }
    }
}
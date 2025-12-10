using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class NoAdsShopElement : BaseShopElement
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private TMP_Text _priceText;

        private IAdsService _adsService;

        [Inject]
        private void Construct(IIapService iapService, IAdsService adsService)
        {
            _iapService = iapService;
            _adsService = adsService;
        }

        public override void Initialize(OfferData data)
        {
            base.Initialize(data);
            _purchaseButton.onClick.Subscribe(() => _iapService.BuyProduct(data.Products[0])).AddTo(_disposables);
            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            var price = GetPrice(data.Products[0]);
            _priceText.text = price; // $"BUY {price}";
        }

        private void OnPurchaseSuccess(string productId)
        {
            if (_adsService.AdsDisabled)
            {
                OnShouldDestroy.OnNext(this);
            }
        }
    }
}

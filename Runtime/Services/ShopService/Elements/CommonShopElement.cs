using System.Collections;
using System.Collections.Generic;
using R3;
using WordGame.Services;
using TapEmpire.Services;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class CommonShopElement : BaseShopElement
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _special;
        [SerializeField] private ResourceUsage _resourceUsage = ResourceUsage.ShopPaid;

        private ProductData _data;
        private ShopSettings _shopSettings;

        [Inject]
        private void Construct(IIapService iapService, IResourcesService resourcesService,
            IUIService uiService, IGameGenericService gameGenericService, IAnimationService animationService)
        {
            _resourcesService = resourcesService;
            _iapService = iapService;
            _uiService = uiService;
            _animationService = animationService;
            _shopSettings = gameGenericService.GameplaySettings.ShopSettings;
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _data = data;
            _purchaseButton.onClick.Subscribe(() => _iapService.BuyProduct(data.Key)).AddTo(_disposables);
            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            var price = GetPrice(data.Key);
            _priceText.text = price; // $"BUY {price}";

            var product = GetProduct<AddResourceProduct>(_data.Key);
            _amount.text = $"x{product.Amount}";
            _icon.sprite = data.Icon;

            if (_special != null)
            {
                _special.gameObject.SetActive(data.InfoType != InfoType.None);
                _special.sprite = _shopSettings.InfoIcons[data.InfoType];
            }
        }

        private void OnPurchaseSuccess(string productId)
        {
            if (_data.Key == productId)
            {
                var from = _icon.transform.position;
                var product = GetProduct<AddResourceProduct>(_data.Key);
                AcquireResources(product.ResourceType, product.Amount, _resourceUsage, from, false);
            }
        }
    }
}

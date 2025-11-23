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
    public class SoftShopElement : BaseShopElement
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] protected Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _special;
        [SerializeField] private CustomButton _customButton;
        [SerializeField] private ResourceUsage _resourceUsage = ResourceUsage.ShopSoft;

        protected ProductData _data;
        private ShopSettings _shopSettings;

        [Inject]
        private void Construct(IResourcesService resourcesService,
            IUIService uiService, IGameGenericService gameGenericService, IAnimationService animationService)
        {
            _resourcesService = resourcesService;
            _uiService = uiService;
            _animationService = animationService;
            _shopSettings = gameGenericService.GameplaySettings.ShopSettings;
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _data = data;
            _purchaseButton.onClick.Subscribe(OnPurchase).AddTo(_disposables);

            _priceText.text = $"{_data.Price.Amount}";

            if (_amount != null)
            {
                _amount.text = $"x{_data.Reward.Amount}";
            }

            if (data.Icon != null)
            {
                _icon.sprite = data.Icon;
            }

            if (_special != null)
            {
                _special.gameObject.SetActive(data.InfoType != InfoType.None);
                _special.sprite = _shopSettings.InfoIcons[data.InfoType];
            }

            _purchaseButton.enabled = HasAmount();
            _customButton.SetActive(_purchaseButton.enabled);

            _resourcesService.GetResourceData(ResourceType.Coins).Amount.Subscribe(OnCoinsChanged).AddTo(_disposables);
        }

        private void OnPurchase()
        {
            if (HasAmount())
            {
                var from = _icon.transform.position;
                _resourcesService.Subtract(_data.Price.Resource, _data.Price.Amount, $"{ResourceUsage.For}{_data.Price.Resource}");
                AcquireResources(_data.Reward.Resource, _data.Reward.Amount, _resourceUsage, from, true);
            }
        }

        private void OnCoinsChanged(int _)
        {
            _purchaseButton.enabled = HasAmount();
            _customButton.SetActive(_purchaseButton.enabled);
        }

        private bool HasAmount()
        {
            return _resourcesService.HasAmount(_data.Price.Resource, _data.Price.Amount);
        }
    }
}

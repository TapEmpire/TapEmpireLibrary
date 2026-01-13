using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class SoftShopElement<ResourceType> : BaseShopElement<ResourceType>
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] protected Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _special;
        [SerializeField] private CustomButton _customButton;
        [SerializeField] private string _placement;

        protected ProductData _data;
        private ShopSettings _shopSettings;

        [Inject]
        private void Construct(IResourcesService<ResourceType> resourcesService, IAnimationService<ResourceType> animationService,
            IUIService uiService, IShopService shopService)
        {
            _resourcesService = resourcesService;
            _uiService = uiService;
            _animationService = animationService;
            _shopSettings = shopService.ShopSettings;
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _data = data;
            _purchaseButton.onClick.Subscribe(OnPurchase).AddTo(_disposables);

            var reward = _data.Reward.As<ProductReward<ResourceType>>();
            var price = _data.Price.As<ProductReward<ResourceType>>();

            _priceText.text = $"{price.Amount}";

            if (_amount != null)
            {
                _amount.text = $"x{reward.Amount}";
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
            _customButton?.SetActive(_purchaseButton.enabled);

            _resourcesService.GetResourceData(price.Resource).Amount.Subscribe(OnCoinsChanged).AddTo(_disposables);
        }

        private void OnPurchase()
        {
            if (HasAmount())
            {
                var reward = _data.Reward.As<ProductReward<ResourceType>>();
                var price = _data.Price.As<ProductReward<ResourceType>>();
                var from = _icon.transform.position;
                _resourcesService.Subtract(price.Resource, price.Amount, $"{ResourceUsageType.For}{reward.Resource}{_placement}");
                AcquireResources(reward.Resource, reward.Amount, $"{ResourceUsageType.For}{price.Resource}{_placement}", from, true);
            }
        }

        private void OnCoinsChanged(int _)
        {
            _purchaseButton.enabled = HasAmount();
            _customButton?.SetActive(_purchaseButton.enabled);
        }

        private bool HasAmount()
        {
            var price = _data.Price.As<ProductReward<ResourceType>>();
            return _resourcesService.HasAmount(price.Resource, price.Amount);
        }
    }
}

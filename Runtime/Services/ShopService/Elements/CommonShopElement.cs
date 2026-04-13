using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class CommonShopElement<ResourceType> : BaseShopElement<ResourceType>
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _special;
        [SerializeField] private string _placement = nameof(ResourceUsageType.ShopPaid);

        private ProductData _data;
        private ShopSettings _shopSettings;

        [Inject]
        private void Construct(IIapService iapService, IResourcesService<ResourceType> resourcesService,
            IUIService uiService, IAnimationService<ResourceType> animationService, IShopService shopService)
        {
            _resourcesService = resourcesService;
            _iapService = iapService;
            _uiService = uiService;
            _animationService = animationService;
            _shopSettings = shopService.ShopSettings;
        }

        public override void Initialize(ProductData data)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                gameObject.SetActive(false);
                return;
            }

            base.Initialize(data);
            _data = data;
            _purchaseButton.onClick.Subscribe(() => _iapService.BuyProduct(data.Key, _placement)).AddTo(_disposables);
            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            var price = GetPrice(data.Key);
            _priceText.text = price; // $"BUY {price}";

            var product = GetProduct<AddResourceProduct<ResourceType>>(_data.Key);
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
                var product = GetProduct<AddResourceProduct<ResourceType>>(_data.Key);
                AcquireResources(product.ResourceType, product.Amount, _placement, from, false, ResourceAcquireType.IAP);
            }
        }
    }
}

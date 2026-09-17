using DG.Tweening;
using R3;
using TapEmpire.Services.Localization;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class ChoiceShopElement<ResourceType> : SoftShopElement<ResourceType>
    {
        public ReactiveCommand<ResourceType> OnResourceAdded { get; } = new();

        [SerializeField] private Button _adsButton;
        [SerializeField] private LocalizeStringEvent _amountLocalization;
        [SerializeField] private LocalizeStringEvent _adsAmountLocalization;

        private IAdsService _adsService;
        private IShopService _shopService;
        private int? _adsAmount;

        [Inject]
        private void Construct2(IAdsService adsService, IShopService shopService)
        {
            _adsService = adsService;
            _shopService = shopService;
        }

        public void Initialize(ProductData data, int adsAmount, bool isAdsEnabled)
        {
            _adsAmount = adsAmount;
            Initialize(data);

            if (!isAdsEnabled)
            {
                _adsButton.gameObject.SetActive(false);
            }
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _adsButton.onClick.Subscribe(OnAdsClick).AddTo(_disposables);

            if (!_adsService.CanShowRewarded || Application.internetReachability == NetworkReachability.NotReachable)
            {
                _adsButton.gameObject.SetActive(false);
            }
        }

        private void OnAdsClick()
        {
            var reward = _data.Reward.As<ProductReward<ResourceType>>();
            _adsService.ShowRewarded($"{AdType.Resources_}{reward.Resource}", OnAdsClickResult);
        }

        protected virtual void OnAdsClickResult()
        {
            var reward = _data.Reward.As<ProductReward<ResourceType>>();
            var from = this != null ? _icon.transform.position : Vector3.zero;
            AcquireResources(reward.Resource, _adsAmount ?? reward.Amount, ResourceUsageType.PopupAds, from, true, ResourceAcquireType.Rewarded);
        }

        protected override void SetAmountText(ProductReward<ResourceType> reward)
        {
            if (_amountLocalization != null)
            {
                _amountLocalization.SetArguments(reward.Amount);
            }

            if (_adsAmountLocalization != null)
            {
                _adsAmountLocalization.SetArguments(_adsAmount ?? reward.Amount);
            }
        }

        protected override void SetPurchaseButtonState()
        {
            PurchaseButton.enabled = true;
        }

        protected override void OnCoinsChanged(int _)
        {
        }

        protected override void OnPurchase()
        {
            if (HasAmount())
            {
                base.OnPurchase();
            }
            else
            {
                _shopService.ShowShop(Placement);
            }
        }

        protected override Sequence AcquireResources(ResourceType resourceType, int amount, string usageType,
            Vector3 startPosition, bool shouldAddResource, ResourceAcquireType acquireType = ResourceAcquireType.Free)
        {
            _resourcesService.Add(resourceType, amount, usageType, acquireType);
            OnResourceAdded.Execute(resourceType);
            return null;
        }
    }
}

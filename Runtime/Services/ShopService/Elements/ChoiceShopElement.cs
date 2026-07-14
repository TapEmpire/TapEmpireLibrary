using DG.Tweening;
using R3;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class ChoiceShopElement<ResourceType> : SoftShopElement<ResourceType>
    {
        public ReactiveCommand<ResourceType> OnResourceAdded { get; } = new();

        [SerializeField] private Button _adsButton;

        private IAdsService _adsService;

        [Inject]
        private void Construct2(IAdsService adsService)
        {
            _adsService = adsService;
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
            AcquireResources(reward.Resource, reward.Amount, ResourceUsageType.PopupAds, from, true, ResourceAcquireType.Rewarded);
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

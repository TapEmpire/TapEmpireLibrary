using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;
using System.Linq;

namespace TapEmpire.Services.Shop
{
    public class StarterOfferUIView<ResourceType> : UIView<NoAdsPopupViewModel>, IInjectable
    {
        [SerializeField] private string _offerName;
        [SerializeField] private List<ShopChoiceData> _offerChoices;
        [SerializeField] private Button _closeButton;

        private IAdsService _adsService;
        private IResourcesService<ResourceType> _resourcesService;
        private IAnimationService<ResourceType> _animationService;

        private OfferData _offerData;
        private bool _shouldEnableBanners;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(IAdsService adsService, IResourcesService<ResourceType> resourcesService,
            IAnimationService<ResourceType> animationService, IShopService shopService)
        {
            _adsService = adsService;
            _resourcesService = resourcesService;
            _animationService = animationService;

            _offerData = shopService.ShopSettings.Offer;
        }

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _shouldEnableBanners = _adsService.ShowBanners(false);
            _closeButton.onClick.Subscribe(DerivedModel.Close).AddTo(_disposables);

            _offerChoices.ForEach((visual, index) =>
            {
                var product = _offerData.Products[index];
                visual.Button.onClick.Subscribe(() => DerivedModel.StartPurchase(product)).AddTo(_disposables);

                var price = DerivedModel.GetPrice(product);
                visual.Price.text = price;

                DerivedModel.IapService.SetResources<ResourceType>(product, visual.Resources.Select(resource => resource.Amount));

                var rewards = DerivedModel.IapService.GetRewards<ResourceType>(product);
                rewards.ForEach((reward, index2) =>
                    visual.Resources[index2].Icon.sprite = _resourcesService.GetFlyingSprite(reward.ResourceType));
            });

            DerivedModel.IapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            return base.OpenAsync(cancellationToken);
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _adsService.ShowBanners(_shouldEnableBanners);
            _disposables.Dispose();
            return base.CloseAsync(cancellationToken);
        }

        private void OnPurchaseSuccess(string productId)
        {
            var index = _offerData.Products.FindIndex(product => product == productId);
            if (index == -1) return;

            var rewards = DerivedModel.IapService.GetRewards<ResourceType>(productId);

            rewards.ForEach((reward, index2) =>
                AcquireResources(reward.ResourceType, reward.Amount, ResourceUsageType.ShopPaid,
                    _offerChoices[index].Resources[index2].Icon.transform.position, false));
        }

        private void AcquireResources(ResourceType resourceType, int amount, string usageType,
            Vector3 startPosition, bool shouldAddResource)
        {
            var animation = _animationService.CollectResource(resourceType, amount, startPosition, false);

            var reason = shouldAddResource ? usageType : string.Empty;
            _resourcesService.AddVirtual(resourceType, amount, reason);

            animation.Play();
        }
    }
}

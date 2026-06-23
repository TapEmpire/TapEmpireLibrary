using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Linq;
using TapEmpire.Services.Shop;
using Sirenix.OdinInspector;

namespace TapEmpire.Services.Offer
{
    public class OfferUIView<ResourceType> : BaseOfferUIView, IInjectable
    {
        [SerializeField] private Image _header;
        [SerializeField] private Image _border;
        [SerializeField] private List<ShopChoiceData> _offerChoices;
        [SerializeField] private Button _closeButton;
        [SerializeField] private bool _disableBanners = false;
        [SerializeField] private Button _debugSwitchButton;
        [SerializeField] private bool _useCustomIcons = false;
        [SerializeField][ShowIf(nameof(_useCustomIcons))] private SerializableDictionary<ResourceType, Sprite> _customIcons;

        protected IOfferService _offerService;
        private IAdsService _adsService;
        private IUIService _uiService;
        private IResourcesService<ResourceType> _resourcesService;
        private IAnimationService<ResourceType> _animationService;

        private bool _shouldEnableBanners;
        private bool _isDebug;
        private ResourcesBar _resourcesBar;
        private CompositeDisposable _disposables = new();
        private CompositeDisposable _visualDisposables = new();

        [Inject]
        private void Construct(IAdsService adsService, IResourcesService<ResourceType> resourcesService,
            IAnimationService<ResourceType> animationService, IOfferService offerService, ISystemService systemService,
            IUIService uiService)
        {
            _adsService = adsService;
            _uiService = uiService;
            _resourcesService = resourcesService;
            _animationService = animationService;
            _offerService = offerService;

            _isDebug = systemService.StaticSettings.Debug;
        }

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            if (_disableBanners)
            {
                _shouldEnableBanners = _adsService.ShowBanner(false);
            }

            _closeButton.onClick.Subscribe(DerivedModel.Close).AddTo(_disposables);
            _debugSwitchButton?.onClick.Subscribe(SwitchRarity).AddTo(_disposables);
            DerivedModel.IapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            _debugSwitchButton?.gameObject.SetActive(_isDebug);

            _resourcesBar = _uiService.ShibariContext.TryGetValue("Resources")?.GetComponent<ResourcesBar>();
            _resourcesBar?.MoveFront();

            SetupVisual();

            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            if (_disableBanners)
            {
                _adsService.ShowBanner(_shouldEnableBanners);
            }

            _visualDisposables.Dispose();
            _disposables.Dispose();
            _resourcesBar?.MoveBack();

            return base.OnCloseAsync(cancellationToken);
        }

        protected virtual IEnumerable<AddResourceProduct<ResourceType>> GetRewards(string productId)
        {
            return DerivedModel.IapService.GetRewards<ResourceType>(productId);
        }

        private void SetupVisual()
        {
            _visualDisposables.Dispose();
            _visualDisposables = new();

            var rarityVisual = _offerService.Settings.Rarity.Visual[DerivedModel.OfferData.Rarity];

            _header.sprite = rarityVisual.Header;
            
            if (_border != null)
            {
                _border.sprite = rarityVisual.Border;
            }

            _offerChoices.ForEach((visual, index) =>
            {
                var product = DerivedModel.OfferData.Products[index];
                visual.Button.onClick.Subscribe(() => DerivedModel.StartPurchase(product)).AddTo(_visualDisposables);

                var price = DerivedModel.GetPrice(product);
                visual.Price.text = price;

                // DerivedModel.IapService.SetResources<ResourceType>(product, visual.Resources.Select(resource => resource.Amount));

                var rewards = GetRewards(product);
                rewards.ForEach((reward, index2) =>
                {
                    visual.Resources[index2].Amount.text = $"x{reward.Amount}";
                    visual.Resources[index2].Icon.sprite = _useCustomIcons ?
                        _customIcons[reward.ResourceType] : _resourcesService.GetFlyingSprite(reward.ResourceType);
                });

                for (int i = rewards.Count(); i < visual.Resources.Count; i++)
                {
                    visual.Resources[i].Icon.transform.parent.gameObject.SetActive(false);
                }
            });
        }

        private void OnPurchaseSuccess(string productId)
        {
            var index = DerivedModel.OfferData.Products.FindIndex(product => product == productId);
            if (index == -1) return;

            var rewards = GetRewards(productId);

            rewards.ForEach((reward, index2) =>
                AcquireResources(reward.ResourceType, reward.Amount, ResourceUsageType.Offer,
                    _offerChoices[index].Resources[index2].Icon.transform.position, false));
        }

        protected virtual void AcquireResources(ResourceType resourceType, int amount, string usageType,
            Vector3 startPosition, bool shouldAddResource)
        {
            var animation = _animationService.CollectResource(resourceType, amount, startPosition, false);

            var reason = shouldAddResource ? usageType : string.Empty;
            _resourcesService.AddVirtual(resourceType, amount, reason, ResourceAcquireType.IAP);

            animation.Play();
        }

        private void SwitchRarity()
        {
            var nextOffer = _offerService.GetOffer(DerivedModel.OfferData.Type, DerivedModel.OfferData.Rarity.Next());
            DerivedModel.SetOfferData(nextOffer.Data);
            SetupVisual();
        }
    }
}

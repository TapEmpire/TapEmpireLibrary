using System;
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
using System.Linq;
using WordGame.Level;

namespace TapEmpire.Services.Shop
{
    public class BundleShopElement : BaseShopElement
    {
        [SerializeField] private List<OfferChoiceData> _offerChoices;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Image _timerProgress;

        private IShopService _shopService;
        private ProjectResourcesSettings _projectResourcesSettings;

        private OfferData _data;
        private UniTaskTimer _timer = null;
        private const float _timerInterval = 1.0f;
        private const float _timerTotal = 24 * 60 * 60;

        [Inject]
        private void Construct(IIapService iapService, IShopService shopService, IUIService uiService,
            IResourcesService resourcesService, IAnimationService animationService, IGameGenericService gameGenericService)
        {
            _iapService = iapService;
            _shopService = shopService;
            _uiService = uiService;
            _resourcesService = resourcesService;
            _animationService = animationService;
            _projectResourcesSettings = gameGenericService.GameplaySettings.ProjectResourcesSettings;
        }

        public override void Initialize(OfferData data)
        {
            base.Initialize(data);
            _data = data;

            _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);

            _offerChoices.ForEach((visual, index) =>
            {
                var product = data.Products[index];
                visual.Button.onClick.Subscribe(() => _iapService.BuyProduct(product.Key)).AddTo(_disposables);

                var price = GetPrice(product.Key);
                visual.Price.text = price;

                _iapService.SetResources<ResourceType>(product.Key, visual.Resources.Select(resource => resource.Amount));

                var rewards = _iapService.GetRewards<ResourceType>(product.Key);
                rewards.ForEach((reward, index2) =>
                    visual.Resources[index2].Icon.sprite = _projectResourcesSettings.GetFlyingSprite(reward.ResourceType));
            });

            UpdateTimer();
        }

        private void OnPurchaseSuccess(string productId)
        {
            var index = _data.Products.FindIndex(product => product.Key == productId);
            if (index == -1) return;

            var rewards = _iapService.GetRewards<ResourceType>(productId);

            rewards.ForEach((reward, index2) =>
                AcquireResources(reward.ResourceType, reward.Amount, ResourceUsage.ShopPaid,
                    _offerChoices[index].Resources[index2].Icon.transform.position, false));

            OnShouldDestroy.OnNext(this);
        }

        private void UpdateTimer()
        {
            var timestamp = _shopService.ActiveOffer.CurrentValue.TimeStamp;
            var elapsed = DateTime.UtcNow - timestamp;

            _timer?.Dispose();
            _timer = UniTaskTimer.Create(_timerTotal, _timerInterval, elapsed.RoundedSeconds());
            _timer.OnTimeLeft.Subscribe(OnTimeLeft);
            _timer.OnTimerDone.Subscribe(OnTimerDone);
        }

        private void OnTimeLeft(float value)
        {
            var time = TimeSpan.FromSeconds(value);
            _timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
            _timerProgress.fillAmount = value / _timerTotal;
        }

        private void OnTimerDone(bool _)
        {
            OnShouldDestroy.OnNext(this);
        }
    }
}

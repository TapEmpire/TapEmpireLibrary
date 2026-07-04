using System;
using DG.Tweening;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class FreeShopElement<ResourceType> : BaseShopElement<ResourceType>
    {
        [SerializeField] protected Button _purchaseButton;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private Button _freeButton;
        [SerializeField] private Button _adsButton;

        [SerializeField] private GameObject _indicator;
        [SerializeField] private GameObject _timerParent;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Image _timerProgress;

        protected ProductData _data;
        private IAdsService _adsService;
        private IShopService _shopService;
        private UniTaskTimer _timer = null;
        private const float _timerInterval = 1.0f;
        private const float _timerTotal = 24 * 60 * 60;

        [Inject]
        private void Construct(IResourcesService<ResourceType> resourcesService, IAdsService adsService, IUIService uiService,
            IShopService shopService, IAnimationService<ResourceType> animationService)
        {
            _resourcesService = resourcesService;
            _adsService = adsService;
            _uiService = uiService;
            _shopService = shopService;
            _animationService = animationService;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _timer?.Dispose();
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _data = data;
            // _purchaseButton.onClick.Subscribe(OnClick).AddTo(_disposables);
            _freeButton.onClick.Subscribe(OnClick).AddTo(_disposables);
            _adsButton.onClick.Subscribe(OnClick).AddTo(_disposables);

            _amount.text = $"x{_data.Reward.As<ProductReward<ResourceType>>().Amount}";
            _icon.sprite = data.Icon;

            UpdateTimer();

            if (_data.Type == ProductType.Ads &&
                (!_adsService.CanShowRewarded || Application.internetReachability == NetworkReachability.NotReachable))
            {
                this.gameObject.SetActive(false);
            }
        }

        private void OnClick()
        {
            if (_data.Type == ProductType.Free)
            {
                OnClickResult(ResourceUsageType.ShopFree, ResourceAcquireType.Free);
            }

            if (_data.Type == ProductType.Ads)
            {
                _adsService.ShowRewarded(AdType.ShopCoins.ToString(), () => OnClickResult(ResourceUsageType.ShopAds, ResourceAcquireType.Rewarded));
            }
        }

        private void OnClickResult(string resourceUsage, ResourceAcquireType acquireType)
        {
            var from = _icon.transform.position;
            var reward = _data.Reward.As<ProductReward<ResourceType>>();
            var sequence = AcquireResources(reward.Resource, reward.Amount, resourceUsage, from, true, acquireType);

            sequence.OnComplete(() => OnAnimationComplete(_data.Type));

            _shopService.SetTimeStamp(_data.Key);
            UpdateTimer();
        }

        private bool UpdateTimer()
        {
            var (isToday, left) = _shopService.HasTimeStampToday(_data.Key);

            if (isToday)
            {
                _timer?.Dispose();
                _timer = UniTaskTimer.Create(_timerTotal, _timerInterval, left.RoundedSeconds());
                _timer.OnTimeLeft.Subscribe(OnTimeLeft);
                _timer.OnTimerDone.Subscribe(SetAvailable);
            }

            SetAvailable(!isToday);

            return isToday;
        }

        private void OnTimeLeft(float value)
        {
            var time = TimeSpan.FromSeconds(value);
            _timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);

            if (_timerProgress != null)
            {
                _timerProgress.fillAmount = value / _timerTotal;
            }
        }

        protected virtual void SetAvailable(bool isAvailable)
        {
            _purchaseButton.enabled = isAvailable;
            _freeButton.gameObject.SetActive(isAvailable && _data.Type == ProductType.Free);
            _adsButton.gameObject.SetActive(isAvailable && _data.Type == ProductType.Ads);
            _timerParent.SetActive(!isAvailable);
            _indicator.SetActive(isAvailable);
        }

        protected virtual void OnAnimationComplete(ProductType productType) { }
    }
}

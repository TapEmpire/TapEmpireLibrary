using System.Threading;
using com.adjust.sdk;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Zenject;
using GoogleMobileAds.Ump.Api;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class AdsService : Initializable, IAdsService
    {
        public System.Action<string> OnAdReceivedRewardEvent { get; set; } = null;
        public System.Action<string> OnAdReceivedOnceRewardEvent { get; set; } = null;
        public System.Action<string> OnAdDisplayedRewardEvent { get; set; } = null;
        public System.Action<string> OnAdClickedEvent { get; set; } = null;
        public System.Action<bool> OnInterstitialAdShowRequested { get; set; } = null;

        public System.Action OnRewardedAdReady { get; set; } = null;

        public bool IsRewardedAdReady => global::AdsManager.Instance.HasAnyRewarded;
        public bool IsInterstitialReady => global::AdsManager.Instance.HasInterstitial;

        [SerializeField]
        private AdsManager _adsManagerPrefab = null;

        [SerializeField]
        private Adjust _adjustPrefab = null;

        [SerializeField]
        private AdsSettings _adsSettings = null;

        // [SerializeField]
        // private AppMetrica _appMetricaPrefab = null;

        [Inject]
        private DiContainer _diContainer = null;

        [SerializeField]
        private bool _adsDisabled = false;
        private string _currentAdPlacement = "";

        [Inject]
        private IProgressService _progressService;

        private Tween _interstitialTimerTween = null;
        private float _interstitialTimer = 30.0f;
        private bool _isInitialized = false;
        private AdsAnalyticsModule _analyticsModule = null;

        public bool AdsDisabled => _adsDisabled;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (_isInitialized)
                return UniTask.CompletedTask;

            if (!_adsDisabled)
            {
                GameObject.Instantiate(_adsManagerPrefab);
                // GameObject.Instantiate(_appMetricaPrefab);
                GameObject.Instantiate(_adjustPrefab);

                _analyticsModule = new AdsAnalyticsModule(_diContainer);
                _analyticsModule.Initialize();

                // global::AdsManager.Instance.OnInitialized += OnInitialized;
                global::AdsManager.Instance.EnableAppOpen = _adsSettings.EnableAppOpen;
                global::AdsManager.Instance.OnConsentObtained += OnConsentObtained;
                global::AdsManager.Instance.Initialize_AdNetworks().ContinueWith(() => PeriodicAdCheck()).Forget();
                _isInitialized = true;
            }

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _isInitialized = false;
            _analyticsModule?.OnRelease();
            _analyticsModule = null;
            _interstitialTimerTween?.Kill();
            _currentAdPlacement = "";

            global::AdsManager.Instance?.OnRelease();
        }

        public void ShowInterstitial(int levelIndex, System.Action callback)
        {
            bool shouldShow = ShouldShowInterstital(levelIndex);

            if (shouldShow && IsInterstitialReady)
            {
                OnAdReceivedOnceRewardEvent = (adType) =>
                {
                    OnAdReceivedOnceRewardEvent = null;
                    callback?.Invoke();
                };
                ShowInterstitial();
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void ShowInterstitial()
        {
            if (_adsDisabled)
            {
                OnAdReceivedReward();
                return;
            }

            if (_currentAdPlacement != "" || !_isInitialized)
            {
                ResetInterstitialByTimer();
                return;
            }

            _currentAdPlacement = AdType_New.Interstital.ToString();
            // OnAdClickedEvent?.Invoke(_currentAdType);
            OnInterstitialAdShowRequested?.Invoke(global::AdsManager.Instance.HasInterstitial);

            global::AdsManager.Instance.ShowInterstitial(() => OnAdReceivedReward(), _currentAdPlacement);
        }

        public void ShowRewarded(string adPlacement)
        {
            if (_adsDisabled)
            {
                OnAdReceivedReward();
                return;
            }

            _currentAdPlacement = adPlacement;
            OnAdClickedEvent?.Invoke(_currentAdPlacement);

            global::AdsManager.Instance.ShowRewarded(() => OnAdReceivedReward(), adPlacement);
        }

        public void ShowAppOpen()
        {
            global::AdsManager.Instance.ShowAppOpen();
        }

        public void DisableAds(bool shouldDisable)
        {
            _adsDisabled = shouldDisable;
            // ProgressManager.SetDisableAds(_adsDisabled);
        }

        public void ShowInterstitialByTimer()
        {
            _interstitialTimerTween?.Kill();
            _interstitialTimerTween = DOVirtual.DelayedCall(_interstitialTimer, ShowInterstitial).SetLoops(-1);
        }

        // Later it might be needed for starting interstitials
        private void OnInitialized()
        {
            // global::AdsManager.Instance.OnInitialized -= OnInitialized;
            _isInitialized = true;
            ResetInterstitialByTimer();
        }

        private void OnConsentObtained(bool isPersonalized)
        {
            global::AdsManager.Instance.OnConsentObtained += OnConsentObtained;
            var firebaseService = _diContainer.Resolve<IFirebaseService>();

            firebaseService.UpdateConsentStatus(isPersonalized);

            GameAnalyticsSDK.GameAnalytics.SetCustomDimension01(ConsentInformation.ConsentStatus.ToString());
        }

        private void ResetInterstitialByTimer()
        {
            if (_interstitialTimerTween == null) return;

            _interstitialTimerTween.Kill();
            _interstitialTimerTween = DOVirtual.DelayedCall(_interstitialTimer, ShowInterstitial).SetLoops(-1);
        }

        private void OnAdReceivedReward()
        {
            ResetInterstitialByTimer();

            OnAdReceivedRewardEvent?.Invoke(_currentAdPlacement);
            OnAdReceivedOnceRewardEvent?.Invoke(_currentAdPlacement);
            _currentAdPlacement = "";
        }

        private void OnRewardedAdLoadedCallback()
        {
            OnRewardedAdReady?.Invoke();
        }

        private void OnAdDisplayedReward()
        {
            OnAdDisplayedRewardEvent?.Invoke(_currentAdPlacement);
        }

        private void PeriodicAdCheck()
        {
            if (this.IsRewardedAdReady)
            {
                OnRewardedAdReady?.Invoke();
            }

            DOVirtual.DelayedCall(1.0f, () => PeriodicAdCheck());
        }

        private bool ShouldShowInterstital(int levelIndex)
        {
            bool shouldShow = _adsSettings.InterstitialAfterLevels.Any(interstitialLevel => interstitialLevel == levelIndex + 1);

            return shouldShow;
        }
    }
}

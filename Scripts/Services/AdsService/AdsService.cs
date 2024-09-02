using System.Threading;
using com.adjust.sdk;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TEL.Services;
using UnityEngine;
using TapEmpire.Services;
using Game.Ads;
using System.Linq;

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

        private bool _adsDisabled = false;
        private string _currentAdPlacement = "";

        private Tween _interstitialTimerTween = null;
        private float _interstitialTimer = 30.0f;
        private bool _isInitialized = false;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (_isInitialized)
                return UniTask.CompletedTask;
            _isInitialized = true;
            GameObject.Instantiate(_adsManagerPrefab);
            // GameObject.Instantiate(_appMetricaPrefab);
            GameObject.Instantiate(_adjustPrefab);
            
            // global::AdsManager.Instance.OnInitialized += OnInitialized;
            global::AdsManager.Instance.Initialize_AdNetworks();
            PeriodicAdCheck();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _isInitialized = false;
            _interstitialTimerTween?.Kill();
        }

        public void ShowInterstitial(int level, System.Action callback)
        {
            bool shouldShow  = _adsSettings.InterstitialAfterLevels.Any(interstitialLevel => interstitialLevel == level);

            if (shouldShow && IsInterstitialReady)
            {
                OnAdReceivedOnceRewardEvent = (adType) => {
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
            _currentAdPlacement = adPlacement;
            OnAdClickedEvent?.Invoke(_currentAdPlacement);

            global::AdsManager.Instance.ShowRewarded(() => OnAdReceivedReward(), adPlacement);
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
    }
}

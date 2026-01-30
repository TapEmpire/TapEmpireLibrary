using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Zenject;
using TapEmpire.Utility;
using R3;
using AdjustSdk;
using Metica.Unity;
using TapEmpire.Modules;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class AdsService : Initializable, IAdsService
    {
        public ReadOnlyReactiveProperty<bool> AdsEnabled => _adsEnabled;
        public Observable<Unit> OnAdsInitialized => global::AdsManager.Instance.OnInitialized;

        public System.Action<string> OnAdReceivedRewardEvent { get; set; } = null;
        public System.Action<string> OnAdReceivedOnceRewardEvent { get; set; } = null;
        public System.Action<string> OnAdDisplayedRewardEvent { get; set; } = null;
        public System.Action<string> OnAdClickedEvent { get; set; } = null;
        public System.Action<bool> OnInterstitialAdShowRequested { get; set; } = null;

        public System.Action OnRewardedAdReady { get; set; } = null;

        public bool IsRewardedAdReady => global::AdsManager.Instance != null && global::AdsManager.Instance.HasAnyRewarded;
        public bool IsInterstitialReady => global::AdsManager.Instance != null && global::AdsManager.Instance.HasInterstitial;

        [SerializeField]
        private AdsManager _adsManagerPrefab = null;

        [SerializeField]
        private Adjust _adjustPrefab = null;

        [SerializeField]
        private MeticaUnitySdk _meticaPrefab = null;

        [SerializeField]
        private AdsSettings _adsSettings = null;

        [Inject]
        private DiContainer _diContainer = null;
        [SerializeField] private bool _adsDisabled;
        private ReactiveProperty<bool> _adsEnabled = new(true);

        private string _currentAdPlacement = "";

        [Inject]
        private IProgressService _progressService;

        private Tween _interstitialTimerTween = null;
        private float _interstitialTimer = 30.0f;
        private bool _isInitialized = false;

        public bool AdsDisabled => _adsDisabled;
        public bool AdsDisabledDebug { get; set; } = false;
        public float MaxWaitingTime => _adsSettings.ShouldWaitAppOpen ? _adsSettings.AppOpenWaitTime : 0.0f;

        private CancellationTokenSource _cancellationTokenSource;

        private ReactiveProperty<bool> _shouldWaitAppOpen = null;
        public ReadOnlyReactiveProperty<bool> ShouldWaitAppOpen { get; private set; } = new ReactiveProperty<bool>(true);
        public AdsSettings Settings => _adsSettings;
        public bool IsMeticaEnabled => global::AdsManager.Instance.IsMeticaEnabled;

        private AdsRuntimeScenario _adsRuntimeScenario;

        private CompositeDisposable _disposables = new();

#if TEL_META
        private FacebookModule _facebookModule = null;
#endif

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (_isInitialized)
                return; //  UniTask.CompletedTask;

            _disposables = new();

            _progressService.TryGetBoolProp(ProgressBoolProp.DisableAds, out _adsDisabled);
            _adsEnabled.Value = !_adsDisabled;
            _adsRuntimeScenario = new AdsRuntimeScenario();
            if (_adsDisabled)
            {
                _adsRuntimeScenario.IsEnabled = false;
                _adsRuntimeScenario.EnableAppOpen = false;
                _adsRuntimeScenario.ShouldWaitAppOpen = false;
                _adsRuntimeScenario.InterstitialAfterLevels = new List<int>();
                _adsRuntimeScenario.ShowBanner = false;
                _adsRuntimeScenario.ShowMrec = false;
                _adsRuntimeScenario.FromLevel = 0;
            }
            else
            {
                _adsRuntimeScenario.IsEnabled = true;
                _adsRuntimeScenario.EnableAppOpen = _adsSettings.EnableAppOpen;
                _adsRuntimeScenario.ShouldWaitAppOpen = _adsSettings.ShouldWaitAppOpen;
                _adsRuntimeScenario.InterstitialAfterLevels = _adsSettings.InterstitialAfterLevels;
                _adsRuntimeScenario.ShowBanner = _adsSettings.EnableBanners && !AdsDisabledDebug;
                _adsRuntimeScenario.ShowMrec = _adsSettings.EnableMrec && !AdsDisabledDebug;
                _adsRuntimeScenario.FromLevel = _adsSettings.FromLevel;
            }

            var level = _progressService.GetLevelProgress();
            var cycles = _progressService.GetCyclesProgress();
            var canEnableBanners = cycles > 0 || _adsSettings.BannerFromLevel <= level + 1;

#if UNITY_IOS
            _adsSettings.EnableMetica = false;
#endif

            if (_adsSettings.EnableMetica)
            {
                GameObject.Instantiate(_meticaPrefab);
            }

            GameObject.Instantiate(_adsManagerPrefab);
            // GameObject.Instantiate(_appMetricaPrefab);

            _adjustPrefab.environment = PlatformInfo.IsTestFlightOrSandboxReceipt() ? AdjustEnvironment.Sandbox : _adjustPrefab.environment;
            GameObject.Instantiate(_adjustPrefab);

            new AdsAnalyticsModule(_diContainer).AddTo(_disposables);
            new AdsSignalsModule(_diContainer).AddTo(_disposables);

            // global::AdsManager.Instance.OnInitialized += OnInitialized;
            global::AdsManager.Instance.EnableAppOpen = _adsRuntimeScenario.EnableAppOpen;
            global::AdsManager.Instance.SetAppOpenAutoShow(true);
            global::AdsManager.Instance.OnConsentObtained += OnConsentObtained;
            global::AdsManager.Instance.Initialize_AdNetworks(_adsSettings, _adsRuntimeScenario, canEnableBanners)
                .ContinueWith(() => PeriodicAdCheck()).Forget();

            _shouldWaitAppOpen = new ReactiveProperty<bool>(_adsRuntimeScenario.ShouldWaitAppOpen);

            ShouldWaitAppOpen = _shouldWaitAppOpen.CombineLatest(global::AdsManager.Instance.ShouldWaitAppOpen,
                (timer, appOpen) => timer && appOpen).ToReadOnlyReactiveProperty();

            _isInitialized = true;

            _cancellationTokenSource = new CancellationTokenSource();
            UniTaskUtility.ExecuteAfterSeconds(MaxWaitingTime,
                () =>
                {
                    _shouldWaitAppOpen.Value = false;
                    global::AdsManager.Instance.ShouldWaitAppOpen.Value = false;
                }, _cancellationTokenSource.Token);

            await UniTask.WaitUntil(() => ShouldWaitAppOpen.CurrentValue == false, cancellationToken: cancellationToken);
            await base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _isInitialized = false;
            _interstitialTimerTween?.Kill();
            _currentAdPlacement = "";

            _disposables.Dispose();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

#if TEL_META
            _facebookModule = null;
#endif

            global::AdsManager.Instance?.OnRelease();
        }

        public void ShowInterstitial(int levelIndex, System.Action callback, string placement = "")
        {
            bool shouldShow = ShouldShowInterstital(levelIndex);

            if (shouldShow && IsInterstitialReady)
            {
                OnAdReceivedOnceRewardEvent = (adType) =>
                {
                    OnAdReceivedOnceRewardEvent = null;
                    callback?.Invoke();
                };

                if (!ShowInterstitial(placement))
                {
                    OnAdReceivedOnceRewardEvent?.Invoke("");
                }
            }
            else
            {
                callback?.Invoke();
            }
        }

        public bool ShowInterstitial(System.Action callback, string placement = "")
        {
            if (!_adsRuntimeScenario.IsEnabled || !IsInterstitialReady)
            {
                callback?.Invoke();
                return false;
            }

            OnAdReceivedOnceRewardEvent = (adType) =>
                {
                    OnAdReceivedOnceRewardEvent = null;
                    callback?.Invoke();
                };

            if (!ShowInterstitial(placement))
            {
                OnAdReceivedOnceRewardEvent.Invoke(string.Empty);
                return false;
            }

            return true;
        }

        public bool ShowInterstitial(string placement = "")
        {
            if (_currentAdPlacement != "" || !_isInitialized)
            {
                ResetInterstitialByTimer();
                return false;
            }

            _currentAdPlacement = string.IsNullOrEmpty(placement) ? AdType_New.Interstital.ToString() : placement;

            if (AdsDisabledDebug)
            {
                OnAdReceivedReward();
                return true;
            }

            // OnAdClickedEvent?.Invoke(_currentAdType);
            OnInterstitialAdShowRequested?.Invoke(global::AdsManager.Instance.HasInterstitial);

            global::AdsManager.Instance.ShowInterstitial(() => OnAdReceivedReward(), _currentAdPlacement);
            return true;
        }

        public bool CanShowRewarded()
        {
            var levelIndex = _progressService.GetLevelProgress();
            return CanShowRewarded(levelIndex);
        }

        public bool CanShowRewarded(int levelIndex)
        {
            var cycles = _progressService.GetCyclesProgress();
            return cycles > 0 || _adsSettings.RewardedFromLevel <= levelIndex + 1;
        }

        public void ShowRewarded(string adPlacement)
        {
            _currentAdPlacement = adPlacement;
            OnAdClickedEvent?.Invoke(_currentAdPlacement);

            if (AdsDisabledDebug)
            {
                OnAdReceivedReward();
                return;
            }

            global::AdsManager.Instance.ShowRewarded(() => OnAdReceivedReward(), adPlacement);
        }

        public void ShowRewarded(string placement, System.Action action)
        {
            OnAdReceivedOnceRewardEvent = (adType) =>
                {
                    OnAdReceivedOnceRewardEvent = null;
                    action?.Invoke();
                };

            ShowRewarded(placement);
        }

        public void ShowAppOpen(System.Action action)
        {
            if (AdsDisabledDebug)
            {
                action?.Invoke();
                return;
            }

            AdsManager.Instance.ShowAppOpen(action);
        }

        public bool ShowBanners(bool shouldShow)
        {
            if (_adsRuntimeScenario.IsEnabled && _adsSettings.EnableBanners)
            {
                var hasBanners = _adsRuntimeScenario.ShowBanner;
                _adsRuntimeScenario.ShowBanner = shouldShow;
                System.Action action = shouldShow ? AdsManager.Instance.ResumeAllBanners : AdsManager.Instance.HideAllBanners;
                action.Invoke();
                return hasBanners;
            }

            return false;
        }

        public void EnableBanners(int levelIndex)
        {
            if (_adsRuntimeScenario.IsEnabled && _adsSettings.EnableBanners)
            {
                var cycles = _progressService.GetCyclesProgress();
                if (cycles > 0 || _adsSettings.BannerFromLevel <= levelIndex + 1)
                {
                    AdsManager.Instance.DoEnableBanner();
                }
            }
        }

        public bool ShowMrec(bool shouldShow)
        {
            if (!_adsRuntimeScenario.IsEnabled || !_adsSettings.EnableMrec)
                return false;

            _adsRuntimeScenario.ShowMrec = shouldShow;
            if (shouldShow)
                AdsManager.Instance.ShowMREC();
            else
                AdsManager.Instance.DestroyMREC();

            return true;
        }

        public bool ShowMrec(bool shouldShow, int x, int y)
        {
            if (!_adsRuntimeScenario.IsEnabled || !_adsSettings.EnableMrec)
                return false;

            _adsRuntimeScenario.ShowMrec = shouldShow;
            if (shouldShow)
                AdsManager.Instance.ShowMREC(x, y);
            else
                AdsManager.Instance.DestroyMREC();

            return true;
        }

        public void DisableAds(bool shouldDisable)
        {
            _adsDisabled = shouldDisable;
            _progressService.SetBoolProp(ProgressBoolProp.DisableAds, _adsDisabled);
            _adsRuntimeScenario.IsEnabled = false;
            _adsRuntimeScenario.EnableAppOpen = false;
            _adsRuntimeScenario.ShouldWaitAppOpen = false;
            _adsRuntimeScenario.InterstitialAfterLevels = new List<int>();
            _adsRuntimeScenario.ShowBanner = false;
            _adsRuntimeScenario.ShowMrec = false;
            _adsRuntimeScenario.FromLevel = 0;
            if (_adsDisabled && AdsManager.Instance != null)
            {
                AdsManager.Instance.DestroyBanner();
                AdsManager.Instance.DestroyMREC();
                AdsManager.Instance.SetAppOpenAutoShow(false);
            }

            _adsEnabled.Value = !_adsDisabled;
        }

        public void ShowInterstitialByTimer()
        {
            _interstitialTimerTween?.Kill();
            _interstitialTimerTween = DOVirtual.DelayedCall(_interstitialTimer, () => ShowInterstitial()).SetLoops(-1);
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

#if TEL_META
            if (_adsSettings.AdsAnalyticsSettings.EnableMeta)
            {
                _facebookModule = new FacebookModule();
                _facebookModule.Initialize(isPersonalized);
            }
#endif

            // GameAnalyticsSDK.GameAnalytics.SetCustomDimension01(ConsentInformation.ConsentStatus.ToString());
        }

        private void ResetInterstitialByTimer()
        {
            if (_interstitialTimerTween == null) return;

            _interstitialTimerTween.Kill();
            _interstitialTimerTween = DOVirtual.DelayedCall(_interstitialTimer, () => ShowInterstitial()).SetLoops(-1);
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
            if (_adsRuntimeScenario.IsEnabled && levelIndex + 1 >= _adsRuntimeScenario.FromLevel)
            {
                return _adsRuntimeScenario.InterstitialAfterLevels.Count == 0 ||
                    _adsRuntimeScenario.InterstitialAfterLevels.Any(interstitialLevel => interstitialLevel == levelIndex + 1);
            }

            return false;
        }
    }
}

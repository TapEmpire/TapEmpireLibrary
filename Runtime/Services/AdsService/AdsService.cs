using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class AdsService : Initializable, IAdsService
    {
        [SerializeField] private AdsSettings _settings;

        public Subject<Unit> OnInitialized { get; } = new();
        public Subject<Unit> OnReceivedReward { get; } = new();
        public Subject<string> OnAdClicked { get; } = new();
        public Subject<bool> OnInterstitialAttempt { get; } = new();
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<AdImpressionData> OnImpressionUnsafe { get; } = new();

        public AdsSettings Settings => _settings;
        public ReadOnlyReactiveProperty<bool> AdsEnabled => _adsEnabled;
        public ReadOnlyReactiveProperty<bool> IsInterstitialReady => _interstitial?.IsLoaded ?? _notReady;
        public ReadOnlyReactiveProperty<bool> IsRewardedReady => _rewarded?.IsLoaded ?? _notReady;
        public bool SkipAds { get; set; }

        public bool CanShowRewarded =>
            _progressService.GetCyclesProgress() > 0 ||
            _progressService.GetLevelProgress() + 1 >= _settings.RewardedFromLevel;

        public bool CanShowInterstitial =>
            _progressService.GetCyclesProgress() > 0 ||
            _progressService.GetLevelProgress() + 1 >= _settings.FromLevel;

        public bool IsMeticaEnabled =>
#if TEL_METICA
            _metica?.IsMeticaEnabled ?? false;
#else
            false;
#endif

        private bool CanShowBanner =>
            _progressService.GetCyclesProgress() > 0 ||
            _progressService.GetLevelProgress() + 1 >= _settings.BannerFromLevel;

        private readonly ReactiveProperty<bool> _notReady = new(false);
        private readonly ReactiveProperty<bool> _adsEnabled = new(true);

        private IConsentService _consentService;
        private IProgressService _progressService;
        private IAnalyticsService _analyticsService;
        private ISystemService _systemService;

        private BannerAdMediator _banner;
        private InterstitialAdMediator _interstitial;
        private RewardedAdMediator _rewarded;
        private MrecAdMediator _mrec;
        private bool _shouldShowBanner = false;

#if TEL_METICA
        private MeticaInitializer _metica;
#endif

        private CompositeDisposable _disposables = new();
        private CompositeDisposable _removableAdsDisposable = new();
        private UniqueDisposable _pendingCallback = new();

        [Inject]
        private void Construct(IConsentService consentService, IProgressService progressService, IAnalyticsService analyticsService, ISystemService systemService)
        {
            _consentService = consentService;
            _progressService = progressService;
            _analyticsService = analyticsService;
            _systemService = systemService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();
            _removableAdsDisposable = new();
            _pendingCallback = new();

            _adsEnabled.Value = !_progressService.GetAdsDisabled();

            InitializeNetworksAsync(LifetimeCancellationToken).Forget();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _removableAdsDisposable.Dispose();
            _disposables.Dispose();
            base.OnRelease();
        }

        public bool ShowBanner(bool shouldShow)
        {
            var hasBanner = _shouldShowBanner;
            _shouldShowBanner = shouldShow && CanShowBanner;
            if (_shouldShowBanner) _banner?.Show();
            else _banner?.Hide();
            return hasBanner;
        }

        public void DisableBanner()
        {
            _banner?.Dispose();
            _banner = null;
        }

        public void ShowInterstitial(string placement, Action onClose, bool skip = false)
        {
            var hasInterstitial = _interstitial?.HasInterstitial(false) == true;
            OnInterstitialAttempt.OnNext(hasInterstitial);

            if (skip || SkipAds || !hasInterstitial)
            {
                onClose?.Invoke();
                return;
            }

            _pendingCallback.Disposable = _interstitial.OnReward.Take(1).Subscribe(_ => onClose?.Invoke());
            _interstitial.Show(placement);
        }

        public void ShowInterstitial(int level, string placement, Action onClose)
        {
            ShowInterstitial(placement, onClose, skip: level < _settings.FromLevel);
        }

        public void ShowRewarded(string placement, Action onRewardCallback)
        {
            OnAdClicked.OnNext(placement);

            _pendingCallback.Disposable = OnReceivedReward.Take(1).Subscribe(_ => onRewardCallback?.Invoke());

            if (SkipAds)
            {
                OnReceivedReward.OnNext(Unit.Default);
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                MobileToast.Show("Sorry, No Internet Connection!", true);
                return;
            }

            _rewarded?.Show(placement);
        }

        public void ShowMrec() => _mrec?.Show();
        public void ShowMrec(int x, int y) => _mrec?.Show(x, y);
        public void HideMrec() => _mrec?.Hide();

        public void DisableAds(bool shouldDisable)
        {
            _progressService.SetAdsDisabled(shouldDisable);
            _adsEnabled.Value = !shouldDisable;
            if (shouldDisable) _removableAdsDisposable.Dispose();
        }

        private async UniTask InitializeNetworksAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log("[Ads] Waiting for consent");
                await _consentService.IsResolved.WaitTrue(cancellationToken);

                var isPersonalized = _consentService.IsPersonalized.CurrentValue;
                var testMode = _settings.Config.TestMode;

                BuildMediators();
                BuildModules();

                Debug.Log("[Ads] Initializing AdMob");
                await AdmobInitializer.Initialize(isPersonalized, _consentService.IsForFamily, cancellationToken);
                AddAdmobProviders();

                Debug.Log("[Ads] Initializing AppLovin Max");
                await MaxInitializer.Initialize(isPersonalized, testMode, cancellationToken);
#if TEL_METICA
                if (_settings.EnableMetica) { _metica = new MeticaInitializer(); await _metica.Initialize(_settings.MeticaPrefab); }
#endif
                AddMaxProviders();

                ShowBanner(_shouldShowBanner);

                Debug.Log("[Ads] Networks initialized");
                OnInitialized.OnNext(Unit.Default);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Ads] Network initialization failed: {exception}");
            }
        }

        private void BuildModules()
        {
            new AdsFirebaseModule(this).AddTo(_disposables);
            new AdsAdjustModule(this).AddTo(_disposables);
            new AdsMetricaModule(this).AddTo(_disposables);
            new AdsAnalyticsModule(this, _analyticsService, _progressService).AddTo(_disposables);
            new AdsFirebaseSignalsModule(this, _progressService).AddTo(_disposables);

            if (_settings.EnableAdSessionGuard)
            {
                new AdSessionGuardModule(_systemService.SystemSettings, _interstitial).AddTo(_disposables);
            }
        }

        private void BuildMediators()
        {
            _rewarded = new RewardedAdMediator();
            _pendingCallback.AddTo(_disposables);
            _rewarded.OnReward.Subscribe(OnReceivedReward.OnNext).AddTo(_disposables);
            SubscribeTo(_rewarded, () => _rewarded = null, _disposables);

            if (_adsEnabled.Value)
            {
                if (_settings.EnableInterstitial)
                {
                    _interstitial = new InterstitialAdMediator();
                    SubscribeTo(_interstitial, () => _interstitial = null, _removableAdsDisposable);
                }

                if (_settings.EnableBanner)
                {
                    _banner = new BannerAdMediator();
                    SubscribeTo(_banner, () => _banner = null, _removableAdsDisposable);
                }

                if (_settings.EnableMrec)
                {
                    _mrec = new MrecAdMediator();
                    SubscribeTo(_mrec, () => _mrec = null, _removableAdsDisposable);
                }
            }
        }

        private void AddAdmobProviders()
        {
            var config = _settings.Config;
            var bannerWidth = _settings.BannerSize == BannerWidth.Full ? 0 : 320;

            _rewarded.AddProvider(new AdmobRewardedProvider(config.Admob.RewardedId, config.TestMode));
            _interstitial?.AddProvider(new AdmobInterstitialProvider(config.Admob.InterstitialId, config.TestMode));
            _banner?.AddProvider(new AdmobBannerProvider(config.Admob.BannerId, _settings.BannerPosition, bannerWidth, config.TestMode));
            _mrec?.AddProvider(new AdmobMrecProvider(config.Admob.MrecId, _settings.MrecPosition, config.TestMode));
        }

        private void AddMaxProviders()
        {
            var config = _settings.Config;
            var bannerWidth = _settings.BannerSize == BannerWidth.Full ? 0 : 320;

            _rewarded.AddProvider(CreateMaxRewarded(config.AppLovin.RewardedId));
            _interstitial?.AddProvider(CreateMaxInterstitial(config.AppLovin.InterstitialId));
            _banner?.AddProvider(new MaxBannerProvider(config.AppLovin.BannerId, _settings.BannerPosition, bannerWidth));
            _mrec?.AddProvider(new MaxMrecProvider(config.AppLovin.MrecId, _settings.MrecPosition));
        }

        private IRewarded CreateMaxRewarded(string adUnitId)
        {
            IRewarded provider = new MaxRewardedProvider(adUnitId);
#if TEL_METICA
            if (_metica != null) provider = new MeticaRewardedProvider(provider, _metica);
#endif
            return provider;
        }

        private IInterstitial CreateMaxInterstitial(string adUnitId)
        {
            IInterstitial provider = new MaxInterstitialProvider(adUnitId);
#if TEL_METICA
            if (_metica != null) provider = new MeticaInterstitialProvider(provider, _metica);
#endif
            return provider;
        }

        private void SubscribeTo<T>(T ad, Action setNull, CompositeDisposable bag) where T : IAd
        {
            ad.AddTo(bag);
            ad.OnImpression.Subscribe(data =>
            {
                OnImpressionUnsafe.OnNext(data);
                UniTaskUtility.RunOnMainThread(() => OnImpression.OnNext(data));
            }).AddTo(bag);
            Disposable.Create(setNull).AddTo(bag);
        }
    }
}

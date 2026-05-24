using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Experimental
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

        private bool CanShowBanner =>
            _progressService.GetCyclesProgress() > 0 ||
            _progressService.GetLevelProgress() + 1 >= _settings.BannerFromLevel;

        private readonly ReactiveProperty<bool> _notReady = new(false);
        private readonly ReactiveProperty<bool> _adsEnabled = new(true);

        private IConsentService _consentService;
        private IProgressService _progressService;
        private IAnalyticsService _analyticsService;

        private IBanner _banner;
        private IInterstitial _interstitial;
        private IRewarded _rewarded;
        private IMrec _mrec;
        private bool _shouldShowBanner = false;

#if TEL_METICA
        private MeticaInitializer _metica;
#endif

        private readonly CompositeDisposable _disposables = new();
        private readonly CompositeDisposable _removableAdsDisposable = new();
        private readonly SerialDisposable _pendingCallback = new();

        [Inject]
        private void Construct(IConsentService consentService, IProgressService progressService, IAnalyticsService analyticsService)
        {
            _consentService = consentService;
            _progressService = progressService;
            _analyticsService = analyticsService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _adsEnabled.Value = !_progressService.GetAdsDisabled();

            InitializeNetworksAsync(cancellationToken).Forget();

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

            _rewarded.Show(placement);
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
            await _consentService.IsResolved.WaitTrue(cancellationToken);

            var isPersonalized = _consentService.IsPersonalized.CurrentValue;
            var testMode = _settings.Config.TestMode;

            await new AdmobInitializer().Initialize(isPersonalized, testMode, cancellationToken);
            await new MaxInitializer().Initialize(isPersonalized, testMode, cancellationToken);
#if TEL_METICA
            if (_settings.EnableMetica) { _metica = new MeticaInitializer(); await _metica.Initialize(_settings.MeticaPrefab); }
#endif

            BuildRewarded();
            if (_adsEnabled.Value) BuildRemovableAds();
            BuildModules();

            OnInitialized.OnNext(Unit.Default);
        }

        private void BuildModules()
        {
            new AdsFirebaseModule(this).AddTo(_disposables);
            new AdsAdjustModule(this).AddTo(_disposables);
            new AdsMetricaModule(this).AddTo(_disposables);
            new AdsAnalyticsModule(this, _analyticsService, _progressService).AddTo(_disposables);
            new AdsFirebaseSignalsModule(this, _progressService).AddTo(_disposables);
        }

        private void BuildRemovableAds()
        {
            if (_settings.EnableInterstitial) BuildInterstitial();
            if (_settings.EnableBanner) BuildBanner();
            if (_settings.EnableMrec) BuildMrec();
        }

        private void BuildRewarded()
        {
            IRewarded appLovinRewarded = new MaxRewardedProvider(_settings.Config.AppLovin.RewardedId);
#if TEL_METICA
            if (_metica != null) appLovinRewarded = new MeticaRewardedProvider(appLovinRewarded, _metica);
#endif
            _rewarded = new RewardedAdMediator(new IRewarded[]
            {
                appLovinRewarded,
                new AdmobRewardedProvider(_settings.Config.Admob.RewardedId),
            });
            _rewarded.AddTo(_disposables);
            _pendingCallback.AddTo(_disposables);
            _rewarded.OnReward.Subscribe(OnReceivedReward.OnNext).AddTo(_disposables);
            _rewarded.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_disposables);
            Disposable.Create(() => _rewarded = null).AddTo(_disposables);
        }

        private void BuildBanner()
        {
            var config = _settings.Config;
            var bannerWidth = _settings.BannerSize == BannerWidth.Full ? 0 : 320;

            _banner = new BannerAdMediator(new IBanner[]
            {
                new MaxBannerProvider(config.AppLovin.BannerId, _settings.BannerPosition, bannerWidth),
                new AdmobBannerProvider(config.Admob.BannerId, _settings.BannerPosition, bannerWidth),
            });
            _banner.AddTo(_removableAdsDisposable);
            _banner.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_removableAdsDisposable);
            Disposable.Create(() => _banner = null).AddTo(_removableAdsDisposable);

            ShowBanner(true);
        }

        private void BuildInterstitial()
        {
            var config = _settings.Config;
            IInterstitial appLovinInterstitial = new MaxInterstitialProvider(config.AppLovin.InterstitialId);
#if TEL_METICA
            if (_metica != null) appLovinInterstitial = new MeticaInterstitialProvider(appLovinInterstitial, _metica);
#endif
            _interstitial = new InterstitialAdMediator(new IInterstitial[]
            {
                appLovinInterstitial,
                new AdmobInterstitialProvider(config.Admob.InterstitialId),
            });
            _interstitial.AddTo(_removableAdsDisposable);
            _interstitial.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_removableAdsDisposable);
            Disposable.Create(() => _interstitial = null).AddTo(_removableAdsDisposable);
        }

        private void BuildMrec()
        {
            var config = _settings.Config;
            _mrec = new MrecAdMediator(new IMrec[]
            {
                new MaxMrecProvider(config.AppLovin.MrecId, _settings.MrecPosition),
                new AdmobMrecProvider(config.Admob.MrecId, _settings.MrecPosition),
            });
            _mrec.AddTo(_removableAdsDisposable);
            _mrec.OnImpression.Subscribe(OnImpression.OnNext).AddTo(_removableAdsDisposable);
            Disposable.Create(() => _mrec = null).AddTo(_removableAdsDisposable);
        }
    }
}

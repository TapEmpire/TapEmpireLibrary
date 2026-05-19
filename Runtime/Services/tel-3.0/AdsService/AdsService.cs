using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.Experimental
{
    [Serializable]
    public class AdsService : Initializable, IAdsService
    {
        private static readonly ReactiveProperty<bool> _notReady = new(false);

        [SerializeField] private AdsSettings _settings;

        [Inject] private IConsentService _consentService;
        [Inject] private IProgressService _progressService;

        private readonly ReactiveProperty<bool> _adsEnabled = new(true);
        private readonly Subject<Unit> _onInitialized = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly CompositeDisposable _paidAdsDisposable = new();

        private IBanner _banner;
        private IInterstitial _interstitial;
        private IRewarded _rewarded;
        private IMrec _mrec;

        public Observable<Unit> OnInitialized => _onInitialized;

        public ReadOnlyReactiveProperty<bool> AdsEnabled => _adsEnabled;
        public ReadOnlyReactiveProperty<bool> IsInterstitialReady => _interstitial?.IsLoaded ?? _notReady;
        public ReadOnlyReactiveProperty<bool> IsRewardedReady => _rewarded?.IsLoaded ?? _notReady;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _adsEnabled.Value = !_progressService.GetAdsDisabled();

            InitializeNetworksAsync(cancellationToken).Forget();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _paidAdsDisposable.Dispose();
            _disposables.Dispose();
            base.OnRelease();
        }

        public void ShowBanner() => _banner?.Show();
        public void HideBanner() => _banner?.Hide();

        public void ShowInterstitial(string placement) => _interstitial?.Show(placement);

        public void ShowRewarded(string placement, Action onReward)
        {
            if (_rewarded == null) return;
            _rewarded.OnReward.Take(1).Subscribe(_ => onReward?.Invoke());
            _rewarded.Show(placement);
        }

        public void ShowMrec() => _mrec?.Show();
        public void ShowMrec(int x, int y) => _mrec?.Show(x, y);
        public void HideMrec() => _mrec?.Hide();

        public void DisableAds(bool shouldDisable)
        {
            _progressService.SetAdsDisabled(shouldDisable);
            _adsEnabled.Value = !shouldDisable;
            if (shouldDisable) _paidAdsDisposable.Dispose();
        }

        private async UniTask InitializeNetworksAsync(CancellationToken cancellationToken)
        {
            await _consentService.IsResolved
                .Where(resolved => resolved)
                .FirstAsync(cancellationToken: cancellationToken);

            var isPersonalized = _consentService.IsPersonalized.CurrentValue;
            var testMode = _settings.Config.TestMode;

            await new AdmobInitializer().Initialize(isPersonalized, testMode, cancellationToken);
            await new MaxInitializer().Initialize(isPersonalized, testMode, cancellationToken);

            BuildRewarded();
            if (_adsEnabled.Value) BuildPaidMediators();

            _onInitialized.OnNext(Unit.Default);
        }

        private void BuildRewarded()
        {
            _rewarded = new RewardedAdMediator(new IRewarded[]
            {
                new MaxRewardedProvider(_settings.Config.AppLovin.RewardedId),
                new AdmobRewardedProvider(_settings.Config.Admob.RewardedId),
            });
            _rewarded.AddTo(_disposables);
            Disposable.Create(() => _rewarded = null).AddTo(_disposables);
        }

        private void BuildPaidMediators()
        {
            if (_settings.EnableBanner) BuildBanner();
            if (_settings.EnableInterstitial) BuildInterstitial();
            if (_settings.EnableMrec) BuildMrec();
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
            _banner.AddTo(_paidAdsDisposable);
            Disposable.Create(() => _banner = null).AddTo(_paidAdsDisposable);

            _banner.Show();
        }

        private void BuildInterstitial()
        {
            var config = _settings.Config;
            _interstitial = new InterstitialAdMediator(new IInterstitial[]
            {
                new MaxInterstitialProvider(config.AppLovin.InterstitialId),
                new AdmobInterstitialProvider(config.Admob.InterstitialId),
            });
            _interstitial.AddTo(_paidAdsDisposable);
            Disposable.Create(() => _interstitial = null).AddTo(_paidAdsDisposable);
        }

        private void BuildMrec()
        {
            var config = _settings.Config;
            _mrec = new MrecAdMediator(new IMrec[]
            {
                new MaxMrecProvider(config.AppLovin.MrecId, _settings.MrecPosition),
                new AdmobMrecProvider(config.Admob.MrecId, _settings.MrecPosition),
            });
            _mrec.AddTo(_paidAdsDisposable);
            Disposable.Create(() => _mrec = null).AddTo(_paidAdsDisposable);
        }
    }
}

using System;
using GoogleMobileAds.Api;
using R3;
using TapEmpire.Utility;
using AdmobAdError = GoogleMobileAds.Api.AdError;

namespace TapEmpire.Services
{
    public class AdmobInterstitialProvider : IInterstitial
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly string _interstitialAdUnitId;

        private InterstitialAd _interstitialAd;
        private IDisposable _adDisposable;
        private CancellableTask _retryDisposable;
        private int _retryAttempt;
        private string _lastPlacement;

        public AdmobInterstitialProvider(string adUnitId, bool testMode)
        {
            _interstitialAdUnitId = testMode ? AdmobTestAdUnits.Interstitial : adUnitId;
            LoadInterstitial();
        }

        public bool HasInterstitial(bool doRequest = false)
        {
            bool isReady = _interstitialAd?.CanShowAd() ?? false;
            if (!isReady && doRequest)
            {
                LoadInterstitial();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            _lastPlacement = placement;
            _interstitialAd?.Show();
        }

        public void Dispose()
        {
            _adDisposable?.Dispose();
            _retryDisposable?.Dispose();
        }

        private void LoadInterstitial()
        {
            _adDisposable?.Dispose();
            InterstitialAd.Load(_interstitialAdUnitId, new AdRequest(), OnLoadCallback);
        }

        private void OnLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            if (error != null || ad == null)
            {
                OnLoadFailed();
                return;
            }

            _retryAttempt = 0;
            _interstitialAd = ad;

            ad.OnAdPaid += OnAdPaid;
            ad.OnAdFullScreenContentClosed += OnAdClosed;
            ad.OnAdFullScreenContentFailed += OnAdShowFailed;

            _adDisposable = Disposable.Create(() =>
            {
                ad.Destroy();
                _interstitialAd = null;
            });

            IsLoaded.Value = true;
        }

        private void OnLoadFailed()
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadInterstitial);
        }

        private void OnAdClosed()
        {
            IsLoaded.Value = false;
            OnReward.OnNext(Unit.Default);
            LoadInterstitial();
        }

        private void OnAdShowFailed(AdmobAdError _)
        {
            IsLoaded.Value = false;
            LoadInterstitial();
        }

        private void OnAdPaid(AdValue value)
        {
            OnImpression.OnNext(AdmobImpressionData.Create(value, AdFormat.Interstitial, _interstitialAdUnitId, _lastPlacement));
        }
    }
}

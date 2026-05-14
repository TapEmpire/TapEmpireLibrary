using System;
using GoogleMobileAds.Api;
using R3;
using TapEmpire.Utility;
using AdmobAdError = GoogleMobileAds.Api.AdError;

namespace TapEmpire.Experimental
{
    public class AdmobInterstitialProvider : IInterstitial, IDisposable
    {
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly string _adUnitId;

        private InterstitialAd _ad;
        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public AdmobInterstitialProvider(string adUnitId)
        {
            _adUnitId = adUnitId;
            Load();
        }

        public bool HasInterstitial(bool doRequest = false)
        {
            bool isReady = _ad != null && _ad.CanShowAd();
            if (!isReady && doRequest)
            {
                Load();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            _ad?.Show();
        }

        public void Dispose()
        {
            DetachAdHandlers();
            _ad?.Destroy();
            _ad = null;
            _retryDisposable?.Dispose();
        }

        private void Load()
        {
            DetachAdHandlers();
            _ad?.Destroy();
            _ad = null;

            InterstitialAd.Load(_adUnitId, new AdRequest(), OnLoadCallback);
        }

        private void OnLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            if (error != null || ad == null)
            {
                _retryAttempt++;
                var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
                _retryDisposable?.Dispose();
                _retryDisposable = UniTaskUtility.Delay(seconds, Load);
                return;
            }

            _retryAttempt = 0;
            _ad = ad;

            _ad.OnAdPaid += OnAdPaid;
            _ad.OnAdFullScreenContentClosed += OnAdClosed;
            _ad.OnAdFullScreenContentFailed += OnAdShowFailed;
        }

        private void DetachAdHandlers()
        {
            if (_ad == null)
            {
                return;
            }

            _ad.OnAdPaid -= OnAdPaid;
            _ad.OnAdFullScreenContentClosed -= OnAdClosed;
            _ad.OnAdFullScreenContentFailed -= OnAdShowFailed;
        }

        private void OnAdClosed()
        {
            OnReward.OnNext(Unit.Default);
            Load();
        }

        private void OnAdShowFailed(AdmobAdError _)
        {
            Load();
        }

        private void OnAdPaid(AdValue value)
        {
            OnImpression.OnNext(new AdImpressionData(
                AdNetwork.Admob,
                "AdMob",
                _adUnitId,
                string.Empty,
                value.Value / 1_000_000d,
                value.CurrencyCode,
                value.Precision.ToString()));
        }
    }
}

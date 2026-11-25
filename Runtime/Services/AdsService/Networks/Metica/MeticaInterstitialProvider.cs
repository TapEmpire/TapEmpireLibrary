#if TEL_METICA
using System;
using Metica;
using Metica.Ads;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public class MeticaInterstitialProvider : IInterstitial
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<bool> IsShowing { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();
        public Subject<AdImpressionData> OnLoaded { get; } = new();
        public Subject<AdImpressionData> OnRecached { get; } = new();

        private readonly string _adUnitId;

        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public MeticaInterstitialProvider(string adUnitId)
        {
            _adUnitId = adUnitId;

            MeticaAdsCallbacks.Interstitial.OnAdLoadSuccess += OnAdLoadSuccess;
            MeticaAdsCallbacks.Interstitial.OnExpiredAdReloaded += OnExpiredAdReloaded;
            MeticaAdsCallbacks.Interstitial.OnAdLoadFailed += OnAdLoadFailed;
            MeticaAdsCallbacks.Interstitial.OnAdShowFailed += OnAdShowFailed;
            MeticaAdsCallbacks.Interstitial.OnAdHidden += OnAdHidden;
            MeticaAdsCallbacks.Interstitial.OnAdRevenuePaid += OnAdRevenuePaid;

            LoadInterstitial();
        }

        public bool HasInterstitial(bool doRequest = false)
        {
            var isReady = MeticaSdk.Ads.IsInterstitialReady(_adUnitId);
            if (!isReady && doRequest)
            {
                LoadInterstitial();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            IsShowing.Value = true;
            MeticaSdk.Ads.ShowInterstitial(_adUnitId, placement);
        }

        public void Dispose()
        {
            MeticaAdsCallbacks.Interstitial.OnAdLoadSuccess -= OnAdLoadSuccess;
            MeticaAdsCallbacks.Interstitial.OnExpiredAdReloaded -= OnExpiredAdReloaded;
            MeticaAdsCallbacks.Interstitial.OnAdLoadFailed -= OnAdLoadFailed;
            MeticaAdsCallbacks.Interstitial.OnAdShowFailed -= OnAdShowFailed;
            MeticaAdsCallbacks.Interstitial.OnAdHidden -= OnAdHidden;
            MeticaAdsCallbacks.Interstitial.OnAdRevenuePaid -= OnAdRevenuePaid;

            _retryDisposable?.Dispose();
        }

        private void LoadInterstitial() => MeticaSdk.Ads.LoadInterstitial(_adUnitId);

        private void OnAdLoadSuccess(MeticaAd ad)
        {
            _retryAttempt = 0;
            IsLoaded.Value = true;
            OnLoaded.OnNext(MeticaImpressionData.Create(ad, AdFormat.Interstitial));
        }

        private void OnExpiredAdReloaded(MeticaAd expiredAd, MeticaAd newAd)
        {
            OnRecached.OnNext(MeticaImpressionData.Create(newAd, AdFormat.Interstitial));
        }

        private void OnAdLoadFailed(MeticaAdError error)
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadInterstitial);
        }

        private void OnAdShowFailed(MeticaAd ad, MeticaAdError error)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            OnReward.OnNext(Unit.Default);
            LoadInterstitial();
        }

        private void OnAdHidden(MeticaAd ad)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            OnReward.OnNext(Unit.Default);
            LoadInterstitial();
        }

        private void OnAdRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Interstitial));
        }
    }
}
#endif

#if TEL_METICA
using System;
using Metica.ADS;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public class MeticaInterstitialProvider : IInterstitial
    {
        public ReactiveProperty<bool> IsLoaded { get; }
        public ReactiveProperty<bool> IsShowing { get; }
        public Subject<AdImpressionData> OnImpression { get; }
        public Subject<Unit> OnReward { get; }

        private readonly IInterstitial _inner;
        private readonly bool _takeover;

        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public MeticaInterstitialProvider(IInterstitial inner, MeticaInitializer initializer)
        {
            _takeover = initializer.IsMeticaEnabled;

            if (_takeover)
            {
                inner.Dispose();
                _inner = null;

                IsLoaded = new ReactiveProperty<bool>(false);
                IsShowing = new ReactiveProperty<bool>(false);
                OnImpression = new Subject<AdImpressionData>();
                OnReward = new Subject<Unit>();

                MeticaAdsCallbacks.Interstitial.OnAdLoadSuccess += OnMeticaLoaded;
                MeticaAdsCallbacks.Interstitial.OnAdLoadFailed += OnMeticaLoadFailed;
                MeticaAdsCallbacks.Interstitial.OnAdShowFailed += OnMeticaShowFailed;
                MeticaAdsCallbacks.Interstitial.OnAdHidden += OnMeticaHidden;
                MeticaAdsCallbacks.Interstitial.OnAdRevenuePaid += OnMeticaRevenuePaid;

                MeticaAds.LoadInterstitial();
            }
            else
            {
                _inner = inner;
                IsLoaded = inner.IsLoaded;
                IsShowing = inner.IsShowing;
                OnImpression = inner.OnImpression;
                OnReward = inner.OnReward;

                // NotifyAdLoadAttempt is not forwarded: IInterstitial has no OnLoadAttempt hook to surface Max's load calls.
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnMaxLoaded;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnMaxLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnMaxRevenuePaid;
            }
        }

        public bool HasInterstitial(bool doRequest = false)
        {
            if (!_takeover) return _inner.HasInterstitial(doRequest);

            var ready = MeticaAds.IsInterstitialReady();
            if (!ready && doRequest) MeticaAds.LoadInterstitial();
            return ready;
        }

        public void Show(string placement)
        {
            if (_takeover)
            {
                IsShowing.Value = true;
                MeticaAds.ShowInterstitial();
            }
            else
            {
                _inner.Show(placement);
            }
        }

        public void Dispose()
        {
            if (_takeover)
            {
                MeticaAdsCallbacks.Interstitial.OnAdLoadSuccess -= OnMeticaLoaded;
                MeticaAdsCallbacks.Interstitial.OnAdLoadFailed -= OnMeticaLoadFailed;
                MeticaAdsCallbacks.Interstitial.OnAdShowFailed -= OnMeticaShowFailed;
                MeticaAdsCallbacks.Interstitial.OnAdHidden -= OnMeticaHidden;
                MeticaAdsCallbacks.Interstitial.OnAdRevenuePaid -= OnMeticaRevenuePaid;
                _retryDisposable?.Dispose();
            }
            else
            {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnMaxLoaded;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnMaxLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnMaxRevenuePaid;
                _inner?.Dispose();
            }
        }

        private void OnMeticaLoaded(MeticaAd ad)
        {
            _retryAttempt = 0;
            IsLoaded.Value = true;
        }

        private void OnMeticaLoadFailed(object error)
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, MeticaAds.LoadInterstitial);
        }

        private void OnMeticaShowFailed(MeticaAd ad, object error)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            MeticaAds.LoadInterstitial();
        }

        private void OnMeticaHidden(MeticaAd ad)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            OnReward.OnNext(Unit.Default);
            MeticaAds.LoadInterstitial();
        }

        private void OnMeticaRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Interstitial));
        }

        private void OnMaxLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo) =>
            MeticaAds.NotifyAdLoadSuccess(adInfo.ToMeticaAd());

        private void OnMaxLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) =>
            MeticaAds.NotifyAdLoadFailed(adUnitId, errorInfo.Message);

        private void OnMaxRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo) =>
            MeticaAds.NotifyAdShowSuccess(adInfo.ToMeticaAd());
    }
}
#endif

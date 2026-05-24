#if TEL_METICA
using System;
using Metica.ADS;
using R3;

namespace TapEmpire.Experimental
{
    public class MeticaInterstitialProvider : IInterstitial
    {
        public ReactiveProperty<bool> IsLoaded { get; }
        public Subject<AdImpressionData> OnImpression { get; }
        public Subject<Unit> OnReward { get; }

        private readonly IInterstitial _inner;
        private readonly bool _takeover;

        public MeticaInterstitialProvider(IInterstitial inner, MeticaInitializer initializer)
        {
            _takeover = initializer.IsAdsEnabled;

            if (_takeover)
            {
                inner.Dispose();
                _inner = null;

                IsLoaded = new ReactiveProperty<bool>(false);
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
                OnImpression = inner.OnImpression;
                OnReward = inner.OnReward;

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
            if (_takeover) MeticaAds.ShowInterstitial();
            else _inner.Show(placement);
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
            }
            else
            {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnMaxLoaded;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnMaxLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnMaxRevenuePaid;
                _inner?.Dispose();
            }
        }

        private void OnMeticaLoaded(MeticaAd ad) => IsLoaded.Value = true;

        private void OnMeticaLoadFailed(object error)
        {
            IsLoaded.Value = false;
            MeticaAds.LoadInterstitial();
        }

        private void OnMeticaShowFailed(MeticaAd ad, object error)
        {
            IsLoaded.Value = false;
            MeticaAds.LoadInterstitial();
        }

        private void OnMeticaHidden(MeticaAd ad)
        {
            IsLoaded.Value = false;
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

#if TEL_METICA
using System;
using Metica.ADS;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public class MeticaRewardedProvider : IRewarded
    {
        public ReactiveProperty<bool> IsLoaded { get; }
        public ReactiveProperty<bool> IsShowing { get; }
        public Subject<AdImpressionData> OnImpression { get; }
        public Subject<Unit> OnReward { get; }

        private readonly IRewarded _inner;
        private readonly bool _takeover;

        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public MeticaRewardedProvider(IRewarded inner, MeticaInitializer initializer)
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

                MeticaAdsCallbacks.Rewarded.OnAdLoadSuccess += OnMeticaLoaded;
                MeticaAdsCallbacks.Rewarded.OnAdLoadFailed += OnMeticaLoadFailed;
                MeticaAdsCallbacks.Rewarded.OnAdShowFailed += OnMeticaShowFailed;
                MeticaAdsCallbacks.Rewarded.OnAdHidden += OnMeticaHidden;
                MeticaAdsCallbacks.Rewarded.OnAdRewarded += OnMeticaRewarded;
                MeticaAdsCallbacks.Rewarded.OnAdRevenuePaid += OnMeticaRevenuePaid;

                MeticaAds.LoadRewarded();
            }
            else
            {
                _inner = inner;
                IsLoaded = inner.IsLoaded;
                IsShowing = inner.IsShowing;
                OnImpression = inner.OnImpression;
                OnReward = inner.OnReward;

                // NotifyAdLoadAttempt is not forwarded: IRewarded has no OnLoadAttempt hook to surface Max's load calls.
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnMaxLoaded;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnMaxLoadFailed;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnMaxRevenuePaid;
            }
        }

        public bool HasRewarded(bool doRequest = false)
        {
            if (!_takeover) return _inner.HasRewarded(doRequest);

            var ready = MeticaAds.IsRewardedReady();
            if (!ready && doRequest) MeticaAds.LoadRewarded();
            return ready;
        }

        public void Show(string placement)
        {
            if (_takeover)
            {
                IsShowing.Value = true;
                MeticaAds.ShowRewarded();
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
                MeticaAdsCallbacks.Rewarded.OnAdLoadSuccess -= OnMeticaLoaded;
                MeticaAdsCallbacks.Rewarded.OnAdLoadFailed -= OnMeticaLoadFailed;
                MeticaAdsCallbacks.Rewarded.OnAdShowFailed -= OnMeticaShowFailed;
                MeticaAdsCallbacks.Rewarded.OnAdHidden -= OnMeticaHidden;
                MeticaAdsCallbacks.Rewarded.OnAdRewarded -= OnMeticaRewarded;
                MeticaAdsCallbacks.Rewarded.OnAdRevenuePaid -= OnMeticaRevenuePaid;
                _retryDisposable?.Dispose();
            }
            else
            {
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnMaxLoaded;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnMaxLoadFailed;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnMaxRevenuePaid;
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
            _retryDisposable = UniTaskUtility.Delay(seconds, MeticaAds.LoadRewarded);
        }

        private void OnMeticaShowFailed(MeticaAd ad, object error)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            MeticaAds.LoadRewarded();
        }

        private void OnMeticaHidden(MeticaAd ad)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            MeticaAds.LoadRewarded();
        }

        private void OnMeticaRewarded(MeticaAd ad)
        {
            OnReward.OnNext(Unit.Default);
        }

        private void OnMeticaRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Rewarded));
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

#if TEL_METICA
using System;
using Metica;
using Metica.Ads;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public class MeticaRewardedProvider : IRewarded
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

        public MeticaRewardedProvider(string adUnitId)
        {
            _adUnitId = adUnitId;

            MeticaAdsCallbacks.Rewarded.OnAdLoadSuccess += OnAdLoadSuccess;
            MeticaAdsCallbacks.Rewarded.OnExpiredAdReloaded += OnExpiredAdReloaded;
            MeticaAdsCallbacks.Rewarded.OnAdLoadFailed += OnAdLoadFailed;
            MeticaAdsCallbacks.Rewarded.OnAdShowFailed += OnAdShowFailed;
            MeticaAdsCallbacks.Rewarded.OnAdHidden += OnAdHidden;
            MeticaAdsCallbacks.Rewarded.OnAdRewarded += OnAdRewarded;
            MeticaAdsCallbacks.Rewarded.OnAdRevenuePaid += OnAdRevenuePaid;

            LoadRewardedAd();
        }

        public bool HasRewarded(bool doRequest = false)
        {
            var isReady = MeticaSdk.Ads.IsRewardedReady(_adUnitId);
            if (!isReady && doRequest)
            {
                LoadRewardedAd();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            IsShowing.Value = true;
            MeticaSdk.Ads.ShowRewarded(_adUnitId, placement);
        }

        public void Dispose()
        {
            MeticaAdsCallbacks.Rewarded.OnAdLoadSuccess -= OnAdLoadSuccess;
            MeticaAdsCallbacks.Rewarded.OnExpiredAdReloaded -= OnExpiredAdReloaded;
            MeticaAdsCallbacks.Rewarded.OnAdLoadFailed -= OnAdLoadFailed;
            MeticaAdsCallbacks.Rewarded.OnAdShowFailed -= OnAdShowFailed;
            MeticaAdsCallbacks.Rewarded.OnAdHidden -= OnAdHidden;
            MeticaAdsCallbacks.Rewarded.OnAdRewarded -= OnAdRewarded;
            MeticaAdsCallbacks.Rewarded.OnAdRevenuePaid -= OnAdRevenuePaid;

            _retryDisposable?.Dispose();
        }

        private void LoadRewardedAd() => MeticaSdk.Ads.LoadRewarded(_adUnitId);

        private void OnAdLoadSuccess(MeticaAd ad)
        {
            _retryAttempt = 0;
            IsLoaded.Value = true;
            OnLoaded.OnNext(MeticaImpressionData.Create(ad, AdFormat.Rewarded));
        }

        private void OnExpiredAdReloaded(MeticaAd expiredAd, MeticaAd newAd)
        {
            OnRecached.OnNext(MeticaImpressionData.Create(newAd, AdFormat.Rewarded));
        }

        private void OnAdLoadFailed(MeticaAdError error)
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadRewardedAd);
        }

        private void OnAdShowFailed(MeticaAd ad, MeticaAdError error)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            LoadRewardedAd();
        }

        private void OnAdHidden(MeticaAd ad)
        {
            IsLoaded.Value = false;
            IsShowing.Value = false;
            LoadRewardedAd();
        }

        private void OnAdRewarded(MeticaAd ad)
        {
            OnReward.OnNext(Unit.Default);
        }

        private void OnAdRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Rewarded));
        }
    }
}
#endif

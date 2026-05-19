using System;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Experimental
{
    public class MaxRewardedProvider : IRewarded
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly string _adUnitId;

        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public MaxRewardedProvider(string adUnitId)
        {
            _adUnitId = adUnitId;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedReward;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;

            LoadRewardedAd();
        }

        public bool HasRewarded(bool doRequest = false)
        {
            bool isReady = MaxSdk.IsRewardedAdReady(_adUnitId);
            if (!isReady && doRequest)
            {
                LoadRewardedAd();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            MaxSdk.ShowRewardedAd(_adUnitId, placement);
        }

        public void Dispose()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedReward;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaid;

            _retryDisposable?.Dispose();
        }

        private void LoadRewardedAd() => MaxSdk.LoadRewardedAd(_adUnitId);

        private void OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _retryAttempt = 0;
            IsLoaded.Value = true;
        }

        private void OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadRewardedAd);
        }

        private void OnAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            IsLoaded.Value = false;
            LoadRewardedAd();
        }

        private void OnAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            IsLoaded.Value = false;
            LoadRewardedAd();
        }

        private void OnAdReceivedReward(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            OnReward.OnNext(Unit.Default);
        }

        private void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnImpression.OnNext(new AdImpressionData(
                AdNetwork.Max,
                adInfo.NetworkName,
                adUnitId,
                adInfo.Placement,
                adInfo.Revenue,
                "USD",
                adInfo.RevenuePrecision));
        }
    }
}

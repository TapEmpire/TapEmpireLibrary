using System;
using R3;
using TapEmpire.Utility;

namespace TapEmpire.Experimental
{
    public class MaxInterstitialProvider : IInterstitial, IDisposable
    {
        public Observable<AdImpressionData> OnImpression => _onImpression;
        public Observable<Unit> OnReward => _onReward;

        private readonly Subject<AdImpressionData> _onImpression = new();
        private readonly Subject<Unit> _onReward = new();
        private readonly string _adUnitId;

        private CancellableTask _retryDisposable;
        private int _retryAttempt;

        public MaxInterstitialProvider(string adUnitId)
        {
            _adUnitId = adUnitId;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;

            LoadInterstitial();
        }

        public bool HasInterstitial(bool doRequest = false)
        {
            bool isReady = MaxSdk.IsInterstitialReady(_adUnitId);
            if (!isReady && doRequest)
            {
                LoadInterstitial();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            MaxSdk.ShowInterstitial(_adUnitId, placement);
        }

        public void Dispose()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnAdDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnAdHidden;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaid;

            _retryDisposable?.Dispose();
        }

        private void LoadInterstitial() => MaxSdk.LoadInterstitial(_adUnitId);

        private void OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _retryAttempt = 0;
        }

        private void OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadInterstitial);
        }

        private void OnAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LoadInterstitial();
        }

        private void OnAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _onReward.OnNext(Unit.Default);
            LoadInterstitial();
        }

        private void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _onImpression.OnNext(new AdImpressionData(
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

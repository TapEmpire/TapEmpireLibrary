using System;
using GoogleMobileAds.Api;
using R3;
using TapEmpire.Utility;
using AdmobAdError = GoogleMobileAds.Api.AdError;

namespace TapEmpire.Services
{
    public class AdmobRewardedProvider : IRewarded
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();
        public Subject<Unit> OnReward { get; } = new();

        private readonly string _rewardedAdUnitId;

        private RewardedAd _rewardedAd;
        private IDisposable _adDisposable;
        private CancellableTask _retryDisposable;
        private int _retryAttempt;
        private string _lastPlacement;

        public AdmobRewardedProvider(string adUnitId, bool testMode)
        {
            _rewardedAdUnitId = testMode ? AdmobTestAdUnits.Rewarded : adUnitId;
            LoadRewarded();
        }

        public bool HasRewarded(bool doRequest = false)
        {
            bool isReady = _rewardedAd?.CanShowAd() ?? false;
            if (!isReady && doRequest)
            {
                LoadRewarded();
            }
            return isReady;
        }

        public void Show(string placement)
        {
            _lastPlacement = placement;
            _rewardedAd?.Show(_ => OnReward.OnNext(Unit.Default));
        }

        public void Dispose()
        {
            _adDisposable?.Dispose();
            _retryDisposable?.Dispose();
        }

        private void LoadRewarded()
        {
            _adDisposable?.Dispose();
            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), OnLoadCallback);
        }

        private void OnLoadCallback(RewardedAd ad, LoadAdError error)
        {
            if (error != null || ad == null)
            {
                OnLoadFailed();
                return;
            }

            _retryAttempt = 0;
            _rewardedAd = ad;

            ad.OnAdPaid += OnAdPaid;
            ad.OnAdFullScreenContentOpened += OnAdOpened;
            ad.OnAdFullScreenContentClosed += OnAdClosed;
            ad.OnAdFullScreenContentFailed += OnAdShowFailed;

            _adDisposable = Disposable.Create(() =>
            {
                ad.Destroy();
                _rewardedAd = null;
            });

            IsLoaded.Value = true;
        }

        private void OnAdOpened()
        {
            FullScreenAdEvents.NotifyOpened();
        }

        private void OnLoadFailed()
        {
            IsLoaded.Value = false;
            _retryAttempt++;
            var seconds = (float)Math.Pow(2, Math.Min(6, _retryAttempt));
            _retryDisposable?.Dispose();
            _retryDisposable = UniTaskUtility.Delay(seconds, LoadRewarded);
        }

        private void OnAdClosed()
        {
            IsLoaded.Value = false;
            FullScreenAdEvents.NotifyClosed();
            LoadRewarded();
        }

        private void OnAdShowFailed(AdmobAdError _)
        {
            IsLoaded.Value = false;
            FullScreenAdEvents.NotifyClosed();
            LoadRewarded();
        }

        private void OnAdPaid(AdValue value)
        {
            OnImpression.OnNext(AdmobImpressionData.Create(value, AdFormat.Rewarded, _rewardedAdUnitId, _lastPlacement));
        }
    }
}

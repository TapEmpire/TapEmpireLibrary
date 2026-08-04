using R3;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MaxBannerProvider : IBanner
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<Vector2> Layout { get; } = new(Vector2.zero);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;

        public MaxBannerProvider(string adUnitId, AdPosition position, int width = 0)
        {
            _adUnitId = adUnitId;

            var configuration = new MaxSdkBase.AdViewConfiguration(position.ToMaxPosition());
            configuration.IsAdaptive = false;
            MaxSdk.CreateBanner(_adUnitId, configuration);
            MaxSdk.StartBannerAutoRefresh(_adUnitId);

            MaxSdk.SetBannerExtraParameter(_adUnitId, "collapsible", "none");
            MaxSdk.SetBannerBackgroundColor(_adUnitId, Color.black);

            if (width > 0)
            {
                MaxSdk.SetBannerWidth(_adUnitId, width);
            }

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        }

        public void Show()
        {
            MaxSdk.ShowBanner(_adUnitId);
            UpdateLayout();
        }

        public void Hide()
        {
            MaxSdk.HideBanner(_adUnitId);
        }

        public void Dispose()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnAdLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnAdLoadFailed;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaid;

            MaxSdk.DestroyBanner(_adUnitId);
            IsLoaded.Value = false;
        }

        private void OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UniTaskUtility.RunOnMainThread(() =>
            {
                IsLoaded.Value = true;
                UpdateLayout();
            });
        }

        private void OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            UniTaskUtility.RunOnMainThread(() =>
            {
                Debug.LogWarning($"[Ads] Max banner load failed: {errorInfo}");
                IsLoaded.Value = false;
            });
        }

        private void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnImpression.OnNext(MaxImpressionData.Create(adInfo, AdFormat.Banner));
        }

        private void UpdateLayout()
        {
            var layout = MaxSdk.GetBannerLayout(_adUnitId);
            Layout.Value = new Vector2(layout.width, layout.height);
        }
    }
}

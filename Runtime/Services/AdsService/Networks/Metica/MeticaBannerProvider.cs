#if TEL_METICA
using Metica;
using Metica.Ads;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MeticaBannerProvider : IBanner
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<Vector2> Layout { get; } = new(Vector2.zero);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;

        public MeticaBannerProvider(string adUnitId, AdPosition position, int width = 0)
        {
            _adUnitId = adUnitId;

            MeticaSdk.Ads.CreateBanner(_adUnitId, new MeticaAdViewConfiguration(position.ToMeticaPosition()));
            MeticaSdk.Ads.StartBannerAutoRefresh(_adUnitId);

            MeticaSdk.Ads.SetBannerExtraParameter(_adUnitId, "collapsible", "none");
            MeticaSdk.Ads.SetBannerBackgroundColor(_adUnitId, "#000000");

            if (width > 0)
            {
                MeticaSdk.Ads.SetBannerWidth(_adUnitId, width);
            }

            MeticaAdsCallbacks.Banner.OnAdLoadSuccess += OnAdLoadSuccess;
            MeticaAdsCallbacks.Banner.OnAdLoadFailed += OnAdLoadFailed;
            MeticaAdsCallbacks.Banner.OnAdRevenuePaid += OnAdRevenuePaid;
        }

        public void Show()
        {
            MeticaSdk.Ads.ShowBanner(_adUnitId);
            UpdateLayout();
        }

        public void Hide()
        {
            MeticaSdk.Ads.HideBanner(_adUnitId);
        }

        public void Dispose()
        {
            MeticaAdsCallbacks.Banner.OnAdLoadSuccess -= OnAdLoadSuccess;
            MeticaAdsCallbacks.Banner.OnAdLoadFailed -= OnAdLoadFailed;
            MeticaAdsCallbacks.Banner.OnAdRevenuePaid -= OnAdRevenuePaid;

            MeticaSdk.Ads.DestroyBanner(_adUnitId);
            IsLoaded.Value = false;
        }

        private void OnAdLoadSuccess(MeticaAd ad)
        {
            IsLoaded.Value = true;
            UpdateLayout();
        }

        private void OnAdLoadFailed(MeticaAdError error)
        {
            Debug.LogWarning($"[Ads] Metica banner load failed: {error}");
            IsLoaded.Value = false;
        }

        private void OnAdRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Banner));
        }

        private void UpdateLayout()
        {
            var layout = MeticaSdk.Ads.GetBannerLayout(_adUnitId);
            Layout.Value = new Vector2((float)layout.Width, (float)layout.Height);
        }
    }
}
#endif

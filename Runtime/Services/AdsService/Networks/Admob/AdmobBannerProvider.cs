using GoogleMobileAds.Api;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public class AdmobBannerProvider : IBanner
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public ReactiveProperty<Vector2> Layout { get; } = new(Vector2.zero);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;
        private readonly BannerView _bannerView;

        public AdmobBannerProvider(string adUnitId, AdPosition position, int width = 0)
        {
            _adUnitId = adUnitId;

            int bannerWidth = width > 0 ? width : AdSize.FullWidth;
            var adSize = new AdSize(bannerWidth, 0);

            _bannerView = new BannerView(_adUnitId, adSize, position.ToAdmobPosition());

            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnBannerAdLoadFailed += OnAdLoadFailed;
            _bannerView.OnAdPaid += OnAdRevenuePaid;

            _bannerView.LoadAd(new AdRequest());
        }

        public void Show()
        {
            _bannerView.Show();
            UpdateLayout();
        }

        public void Hide()
        {
            _bannerView.Hide();
        }

        public void Dispose()
        {
            _bannerView.OnBannerAdLoaded -= OnAdLoaded;
            _bannerView.OnBannerAdLoadFailed -= OnAdLoadFailed;
            _bannerView.OnAdPaid -= OnAdRevenuePaid;

            _bannerView.Destroy();
            IsLoaded.Value = false;
        }

        private void OnAdLoaded()
        {
            IsLoaded.Value = true;
            UpdateLayout();
        }

        private void OnAdLoadFailed(LoadAdError _)
        {
            IsLoaded.Value = false;
        }

        private void OnAdRevenuePaid(AdValue value)
        {
            OnImpression.OnNext(AdmobImpressionData.Create(value, AdFormat.Banner, _adUnitId));
        }

        private void UpdateLayout()
        {
            float w = _bannerView.GetWidthInPixels();
            float h = _bannerView.GetHeightInPixels();
            Layout.Value = new Vector2(w, h);
        }
    }
}

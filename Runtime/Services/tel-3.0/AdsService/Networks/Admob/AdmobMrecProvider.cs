using System;
using GoogleMobileAds.Api;
using R3;

namespace TapEmpire.Experimental
{
    public class AdmobMrecProvider : IMrec, IDisposable
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private static readonly AdSize Size = new(300, 250);

        private readonly string _adUnitId;
        private readonly AdPosition _position;
        private BannerView _mrecView;

        public AdmobMrecProvider(string adUnitId, AdPosition position)
        {
            _adUnitId = adUnitId;
            _position = position;
        }

        public void Show()
        {
            Teardown();
            Attach(new BannerView(_adUnitId, Size, _position.ToAdmobPosition()));
        }

        public void Show(int x, int y)
        {
            Teardown();
            Attach(new BannerView(_adUnitId, Size, x, y));
        }

        public void Hide()
        {
            _mrecView?.Hide();
        }

        public void Dispose()
        {
            Teardown();
        }

        private void Teardown()
        {
            Detach();
            _mrecView?.Destroy();
            _mrecView = null;
            IsLoaded.Value = false;
        }

        private void Attach(BannerView view)
        {
            _mrecView = view;
            _mrecView.OnBannerAdLoaded += OnAdLoaded;
            _mrecView.OnBannerAdLoadFailed += OnAdLoadFailed;
            _mrecView.OnAdPaid += OnAdRevenuePaid;
            _mrecView.LoadAd(new AdRequest());
        }

        private void Detach()
        {
            if (_mrecView == null)
            {
                return;
            }
            _mrecView.OnBannerAdLoaded -= OnAdLoaded;
            _mrecView.OnBannerAdLoadFailed -= OnAdLoadFailed;
            _mrecView.OnAdPaid -= OnAdRevenuePaid;
        }

        private void OnAdLoaded()
        {
            IsLoaded.Value = true;
        }

        private void OnAdLoadFailed(LoadAdError _)
        {
            IsLoaded.Value = false;
        }

        private void OnAdRevenuePaid(AdValue value)
        {
            OnImpression.OnNext(new AdImpressionData(
                AdNetwork.Admob,
                "AdMob",
                _adUnitId,
                string.Empty,
                value.Value / 1_000_000d,
                value.CurrencyCode,
                value.Precision.ToString()));
        }
    }
}

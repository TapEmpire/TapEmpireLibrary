#if TEL_METICA
using Metica;
using Metica.Ads;
using R3;

namespace TapEmpire.Services
{
    public class MeticaMrecProvider : IMrec
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;

        public MeticaMrecProvider(string adUnitId, AdPosition position)
        {
            _adUnitId = adUnitId;

            MeticaSdk.Ads.CreateMrec(_adUnitId, new MeticaAdViewConfiguration(position.ToMeticaPosition()));

            MeticaAdsCallbacks.Mrec.OnAdLoadSuccess += OnAdLoadSuccess;
            MeticaAdsCallbacks.Mrec.OnAdLoadFailed += OnAdLoadFailed;
            MeticaAdsCallbacks.Mrec.OnAdRevenuePaid += OnAdRevenuePaid;
        }

        public void Show()
        {
            MeticaSdk.Ads.ShowMrec(_adUnitId);
        }

        public void Show(int x, int y)
        {
            var (dpX, dpY) = ConvertToDp(x, y);
            MeticaSdk.Ads.UpdateMrecPositionCoordinates(_adUnitId, dpX, dpY);
            MeticaSdk.Ads.ShowMrec(_adUnitId);
        }

        public void Hide()
        {
            MeticaSdk.Ads.HideMrec(_adUnitId);
        }

        public void Dispose()
        {
            MeticaAdsCallbacks.Mrec.OnAdLoadSuccess -= OnAdLoadSuccess;
            MeticaAdsCallbacks.Mrec.OnAdLoadFailed -= OnAdLoadFailed;
            MeticaAdsCallbacks.Mrec.OnAdRevenuePaid -= OnAdRevenuePaid;

            MeticaSdk.Ads.DestroyMrec(_adUnitId);
            IsLoaded.Value = false;
        }

        private void OnAdLoadSuccess(MeticaAd ad)
        {
            IsLoaded.Value = true;
        }

        private void OnAdLoadFailed(MeticaAdError error)
        {
            IsLoaded.Value = false;
        }

        private void OnAdRevenuePaid(MeticaAd ad)
        {
            OnImpression.OnNext(MeticaImpressionData.Create(ad, AdFormat.Mrec));
        }

        private static (int x, int y) ConvertToDp(int pixelX, int pixelY)
        {
            float density = MaxSdkUtils.GetScreenDensity();
            int dpX = (int)(pixelX / density);
            int dpY = (int)(pixelY / density);
            return (dpX, dpY);
        }
    }
}
#endif

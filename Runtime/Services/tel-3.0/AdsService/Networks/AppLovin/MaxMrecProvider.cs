using System;
using R3;

namespace TapEmpire.Experimental
{
    public class MaxMrecProvider : IMrec, IDisposable
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;

        public MaxMrecProvider(string adUnitId, MaxSdkBase.AdViewPosition position)
        {
            _adUnitId = adUnitId;

            var configuration = new MaxSdkBase.AdViewConfiguration(position);
            MaxSdk.CreateMRec(_adUnitId, configuration);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        }

        public void Show()
        {
            MaxSdk.ShowMRec(_adUnitId);
        }

        public void Show(int x, int y)
        {
            var (dpX, dpY) = ConvertToDp(x, y);
            MaxSdk.UpdateMRecPosition(_adUnitId, dpX, dpY);
            MaxSdk.ShowMRec(_adUnitId);
        }

        public void Hide()
        {
            MaxSdk.HideMRec(_adUnitId);
        }

        public void Destroy()
        {
            MaxSdk.DestroyMRec(_adUnitId);
            IsLoaded.Value = false;
        }

        public void Dispose()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent -= OnAdLoaded;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent -= OnAdLoadFailed;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= OnAdRevenuePaid;

            Destroy();
        }

        private void OnAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            IsLoaded.Value = true;
        }

        private void OnAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            IsLoaded.Value = false;
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

        private static (int x, int y) ConvertToDp(int pixelX, int pixelY)
        {
            float density = MaxSdkUtils.GetScreenDensity();
            int dpX = (int)(pixelX / density);
            int dpY = (int)(pixelY / density);
            return (dpX, dpY);
        }
    }
}

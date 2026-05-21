using R3;

namespace TapEmpire.Experimental
{
    public class MaxMrecProvider : IMrec
    {
        public ReactiveProperty<bool> IsLoaded { get; } = new(false);
        public Subject<AdImpressionData> OnImpression { get; } = new();

        private readonly string _adUnitId;

        public MaxMrecProvider(string adUnitId, AdPosition position)
        {
            _adUnitId = adUnitId;

            var configuration = new MaxSdkBase.AdViewConfiguration(position.ToMaxPosition());
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

        public void Dispose()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent -= OnAdLoaded;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent -= OnAdLoadFailed;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= OnAdRevenuePaid;

            MaxSdk.DestroyMRec(_adUnitId);
            IsLoaded.Value = false;
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
            OnImpression.OnNext(MaxImpressionData.Create(adInfo, AdFormat.Mrec));
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

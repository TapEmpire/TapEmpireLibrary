using Firebase.Analytics;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    internal class AdsFirebaseSignalsModule : AdsSignalsModuleBase
    {
        public AdsFirebaseSignalsModule(IAdsService adsService, IProgressService progressService)
            : base(adsService, progressService)
        {
        }

        protected override BatchedData InitializeBatchedData() =>
            new()
            {
                BatchType = _analyticsSettings.BatchType,
                Threshold = _analyticsSettings.Threshold,
                Postfix = "",
            };

        protected override LayeredData InitializeLayeredData() =>
            new()
            {
                RevenueLayers = _analyticsSettings.RevenueLayers,
                Postfix = "",
            };

        protected override void LogBatched(double revenue)
        {
            var parameters = new[]
            {
                new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
            };
            FirebaseService.LogEvent("ad_revenue_batched", parameters);
        }

        protected override void LogLayered(string name, double revenue)
        {
            FirebaseService.LogEvent(name);
        }

        protected override void OnImpression(AdImpressionData data)
        {
            base.OnImpression(data);
            UpdateCounter(data);
        }

        private void UpdateCounter(AdImpressionData data)
        {
            if (!_analyticsSettings.AddBanners && !data.Format.IsExpensive())
            {
                return;
            }

            var counter = _progressService.GetAdCounter() + 1;
            if (counter >= _analyticsSettings.CounterThreshold)
            {
                FirebaseService.LogEvent("ad_counter", new[]
                {
                    new Parameter(FirebaseAnalytics.ParameterValue, counter),
                });
                counter = 0;
            }
            _progressService.SetAdCounter(counter);
        }
    }
}

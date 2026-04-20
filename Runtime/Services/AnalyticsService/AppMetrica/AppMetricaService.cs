using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Io.AppMetrica;
using Newtonsoft.Json;
using R3;

namespace TapEmpire.Services
{
    public class AppMetricaService : Initializable, IAnalyticsService
    {
        public ReadOnlyReactiveProperty<string> CampaignName => null;
        private Dictionary<string, object> _globalParameters = new();

        public AppMetricaService(string analyticsKey, bool shouldEnableLogs)
        {
            AppMetrica.Activate(new AppMetricaConfig(analyticsKey)
            {
                Logs = shouldEnableLogs,
                // CrashReporting = false,
            });
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            var dictionary = eventParams != null ?
                _globalParameters.Concat(eventParams).ToDictionary(pair => pair.Key, pair => pair.Value) :
                _globalParameters;
            var json = JsonConvert.SerializeObject(dictionary);
            AppMetrica.ReportEvent(eventName, json);
        }

        public void LogEvent(string eventName, int value)
        {
            var dictionary = new Dictionary<string, object>(_globalParameters);
            dictionary["value"] = value;
            LogEvent(eventName, dictionary);
        }

        public void LogProgressionEvent(ProgressionState state, string progression01, string progression02, string progression03)
        {
        }

        public void SetUserProperty(string propertyName, int value)
        {
            _globalParameters[propertyName] = value;
        }

        public void SetUserProperty(string propertyName, string value, bool everywhere = false)
        {
            _globalParameters[propertyName] = value;
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            foreach (var pair in properties)
            {
                _globalParameters[pair.Key] = pair.Value;
            }
        }

        public void FlushEvents()
        {
            AppMetrica.SendEventsBuffer();
        }

        public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            var json = JsonConvert.SerializeObject(details ?? new Dictionary<string, object>());
            AppMetrica.ReportEvent(eventName, json);
        }

        public void LogAdjustEvent(IDictionary<string, object> properties)
        {
        }

        public void SetCampaignName(string campaignName) { }
    }
}
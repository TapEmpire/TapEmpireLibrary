using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public class AmplitudeService : Initializable, IAnalyticsService
    {
        private Amplitude _amplitude = null;
        private string _amplitudeKey = "";
        private bool _logAmplitude = false;

        public AmplitudeService(string amplitudeKey, bool logAmplitude)
        {
            _amplitudeKey = amplitudeKey; // settingsManager.BuildSettings.AmplitudeKey;
            _logAmplitude = logAmplitude;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var amplitudeKey = _amplitudeKey;
            InitializeBasic(amplitudeKey);

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _amplitude = null;
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            _amplitude.logEvent(eventName, eventParams);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            _amplitude.setUserProperty(propertyName, value);
        }

        public void SetUserProperty(string propertyName, string value)
        {
            _amplitude.setUserProperty(propertyName, value);
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            _amplitude.setUserProperties(properties);
        }

        public void FlushEvents()
        {
            _amplitude.uploadEvents();
        }

        private void InitializeBasic(string amplitudeKey)
        {
            _amplitude = Amplitude.getInstance();
            _amplitude.setServerUrl("https://api2.amplitude.com");
            _amplitude.logging = _logAmplitude;
            _amplitude.trackSessionEvents(true);
            _amplitude.init(amplitudeKey);
        }

        public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            Amplitude amplitude = Amplitude.getInstance();
            amplitude.logEvent(eventName, details);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using com.adjust.sdk;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Utility;
using Zenject;
using Object = UnityEngine.Object;

namespace TapEmpire.Services
{
    [Serializable]
    public class AnalyticsService : Initializable, IAnalyticsService
    {
        private static readonly Dictionary<string, object> EmptyDictionary = new();

        [SerializeField]
        private string _amplitudeKey = "";

        [SerializeField]
        private MonoCallbacksService _monoCallbackServicePrefab = null;

        [SerializeField]
        private bool _logAmplitude;

        [Inject]
        private DiContainer _diContainer = null;

        private Amplitude _amplitude = null;
        private bool _isInitialized = false;
        private List<Action> _delayedEvents = new();
        private MonoCallbacksService _monoCallbackService = null;
        private Adjust _adjust = null;

        // [NonSerialized]
        // private AnalyticsGlobalModule _globalModule;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var amplitudeKey = _amplitudeKey; // settingsManager.BuildSettings.AmplitudeKey;
            InitializeBasic(amplitudeKey);

            if (_monoCallbackService == null)
            {
                _monoCallbackService = Object.Instantiate(_monoCallbackServicePrefab);
                Object.DontDestroyOnLoad(_monoCallbackService.gameObject);
            }

            _adjust = Object.FindObjectOfType<Adjust>();

            // _globalModule = new AnalyticsGlobalModule(_diContainer, LogGlobalEvent);
            // _globalModule.Initialize();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _isInitialized = false;
            _diContainer.Resolve<IABTestingService>().OnGroupAssigned -= onGroupAssigned;
            _monoCallbackService.OnApplicationFocusChange -= OnApplicationFocus;

            // AdsModule.OnRelease();

            if (_adjust != null)
                _adjust.OnConfigChanged -= OnConfigChanged;

            //_globalModule = null;
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            logEventDelayed(eventName, eventParams);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            SetUserIntProperty(propertyName, value);
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            if (_isInitialized)
            {
                _amplitude.setUserProperties(properties);
            }
        }

        private void onGroupAssigned()
        {
            InitializeDeferred();
            _diContainer.Resolve<IABTestingService>().OnGroupAssigned -= onGroupAssigned;
        }

        private void InitializeBasic(string amplitudeKey)
        {
            _amplitude = Amplitude.getInstance();
            _amplitude.setServerUrl("https://api2.amplitude.com");
            _amplitude.logging = _logAmplitude;
            _amplitude.trackSessionEvents(true);
            _amplitude.init(amplitudeKey);

            _diContainer.Resolve<IABTestingService>().OnGroupAssigned += onGroupAssigned;
        }

        private void InitializeDeferred()
        {
            var (isFirstLaunch, launchDate) = PlayerPrefsUtility.GetFirstLaunch();

            var daysAfterInstall = DateTime.UtcNow - launchDate;

            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            var cyclesCompleted = progressService.GetCyclesProgress();
            var adsWatchedCount = progressService.GetAdsWatchedProgress();

            _amplitude.setUserProperties(new Dictionary<string, object>{
                { AnalyticsParameters.InstallYear, launchDate.Year },
                { AnalyticsParameters.InstallDate, launchDate.DayOfYear },
                { AnalyticsParameters.DaysAfterInstall, daysAfterInstall.Days },
                { AnalyticsParameters.AdjustAttribution, Adjust.getAttribution()?.network},
                { AnalyticsParameters.RemoteConfig, progressService.GetRemoteConfigName() },
                { CoreGenericAnalyticsParameters.LevelsCompleted, levelsCompleted },
                { CoreGenericAnalyticsParameters.CyclesCompleted, cyclesCompleted },
                { AdsAnalyticsParameters.AdsWatched, adsWatchedCount }
                //{ AnalyticsParameters.AbGroup1, _diContainer.Resolve<IABTestingService>().Group },
            });

            if (isFirstLaunch)
            {
                _amplitude.logEvent(AnalyticsEvents.LaunchFirstTime);
            }

            // AdsService.Initialize();

            _adjust.OnConfigChanged += OnConfigChanged;

            _monoCallbackService.OnApplicationFocusChange += OnApplicationFocus;
            OnApplicationFocus(true); // Hack

            _isInitialized = true;
            _delayedEvents.ForEach(x => x.Invoke());
            _delayedEvents.Clear();
        }

        private void OnConfigChanged(AdjustAttribution attribution)
        {
            _amplitude.setUserProperty(AnalyticsParameters.AdjustAttribution, attribution.network);
        }

        public void logEventDelayed(string eventName, Dictionary<string, object> parameters = null)
        {
            Action delayedEvent = () => this._amplitude.logEvent(eventName, parameters);
            Action action = _isInitialized ? delayedEvent : () => _delayedEvents.Add(delayedEvent);
            action.Invoke();
        }

        public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            Amplitude amplitude = Amplitude.getInstance();
            amplitude.logEvent(eventName, details ?? EmptyDictionary);
        }

        private void SetUserIntProperty(string propertyName, int value)
        {
            if (_isInitialized)
            {
                _amplitude.setUserProperty(propertyName, value);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                logEventDelayed(AnalyticsEvents.SessionStart);
                PlayerPrefsUtility.SetSessionStart();
            }
            else
            {
                PlayerPrefsUtility.SetSessionEnd();
                logEventDelayed(AnalyticsEvents.SessionEnd);
                _amplitude.uploadEvents();
            }
        }
    }
}
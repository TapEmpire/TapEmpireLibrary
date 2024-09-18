using System;
using System.Collections.Generic;
using System.Threading;
using com.adjust.sdk;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Utility;
using Zenject;
using Object = UnityEngine.Object;
using Sirenix.OdinInspector;

namespace TapEmpire.Services
{
    [Serializable]
    public class AnalyticsService : Initializable, IAnalyticsService
    {
        private static readonly Dictionary<string, object> EmptyDictionary = new();

        [SerializeField]
        private MonoCallbacksService _monoCallbackServicePrefab = null;

        [SerializeField]
        private AnalyticsType _analyticsType = AnalyticsType.Amplitude;

        [SerializeField]
        [ShowIf("@_analyticsType == AnalyticsType.Amplitude")]
        private string _amplitudeKey = "";

        [SerializeField]
        [ShowIf("@_analyticsType == AnalyticsType.Amplitude")]
        private bool _logAmplitude;

        [SerializeField]
        [ShowIf("@_analyticsType == AnalyticsType.GameAnalytics")]
        private GameAnalyticsSDK.GameAnalytics _gameAnalyticsPrefab;

        private DiContainer _diContainer = null;
        private IProgressService _progressService = null;

        private bool _isInitialized = false;
        private List<Action> _delayedEvents = new();
        private MonoCallbacksService _monoCallbackService = null;
        private Adjust _adjust = null;
        private IAnalyticsService _innerService = null;

        // [NonSerialized]
        // private AnalyticsGlobalModule _globalModule;

        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _innerService = _analyticsType == AnalyticsType.Amplitude ?
                new AmplitudeService(_amplitudeKey, _logAmplitude) : new GameAnalyticsService(_gameAnalyticsPrefab);

            _innerService.InitializeAsync(cancellationToken);
            _diContainer.Resolve<IABTestingService>().OnGroupAssigned += onGroupAssigned;

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

            _innerService.Release();

            //_globalModule = null;
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            logEventDelayed(eventName, eventParams);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperty(propertyName, value);
            }
        }

        public void SetUserProperty(string propertyName, string value)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperty(propertyName, value);
            }
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperties(properties);
            }
        }

        private void onGroupAssigned()
        {
            _diContainer.Resolve<IABTestingService>().OnGroupAssigned -= onGroupAssigned;
            InitializeDeferred();
        }

        private void InitializeDeferred()
        {
            var (isFirstLaunch, launchDate) = PlayerPrefsUtility.GetFirstLaunch();

            var daysAfterInstall = DateTime.UtcNow - launchDate;

            var progressService = _diContainer.Resolve<IProgressService>();
            var levelsCompleted = progressService.GetLevelProgress();
            var cyclesCompleted = progressService.GetCyclesProgress();
            var adsWatchedCount = progressService.GetAdsWatchedProgress();

            _innerService.SetUserProperties(new Dictionary<string, object>{
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
                _innerService.LogEvent(AnalyticsEvents.LaunchFirstTime, null);
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
            _innerService.SetUserProperty(AnalyticsParameters.AdjustAttribution, attribution.network);
        }

        public void logEventDelayed(string eventName, Dictionary<string, object> parameters = null)
        {
            Action delayedEvent = () => _innerService.LogEvent(eventName, parameters);
            Action action = _isInitialized ? delayedEvent : () => _delayedEvents.Add(delayedEvent);
            action.Invoke();
        }

        public void FlushEvents()
        {
            _innerService.FlushEvents();
        }

        /*public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            Amplitude amplitude = Amplitude.getInstance();
            amplitude.logEvent(eventName, details ?? EmptyDictionary);
        }*/

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                logEventDelayed(AnalyticsEvents.SessionStart);
                PlayerPrefsUtility.SetSessionStart();
                _progressService.UpdateSessionsStarted();
            }
            else
            {
                PlayerPrefsUtility.SetSessionEnd();
                logEventDelayed(AnalyticsEvents.SessionEnd);
                FlushEvents();
            }
        }
    }
}
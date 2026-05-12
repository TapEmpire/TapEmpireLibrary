using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Utility;
using TapEmpire.Experimental;
using Zenject;
using Sirenix.OdinInspector;
using R3;
using TapEmpire.Modules;

namespace TapEmpire.Services
{
    [Serializable]
    public class AnalyticsService : Initializable, IAnalyticsService
    {
        public ReadOnlyReactiveProperty<string> CampaignName => _campaignName;

        [field: SerializeField] public string AdjustEventToken { get; private set;}

        [SerializeField]
        private AnalyticsType _analyticsType = AnalyticsType.Amplitude;

        [SerializeField]
        [ShowIf("@_analyticsType == AnalyticsType.Amplitude || _analyticsType == AnalyticsType.AppMetrica")]
        private string _analyticsKey = "";

        [SerializeField]
        [ShowIf("@_analyticsType == AnalyticsType.Amplitude || _analyticsType == AnalyticsType.AppMetrica")]
        private bool _shouldEnableLogs = false;

        private ReactiveProperty<string> _campaignName = new();

        private DiContainer _diContainer = null;
        private IProgressService _progressService = null;
        private ISystemService _systemService = null;
        private IAttributionService _attributionService = null;

        private bool _isInitialized = false;
        private List<Action> _delayedEvents = new();
        private IAnalyticsService _innerService = null;
        private string _remoteConfigName = "default";

        private Dictionary<string, string> _globalParameters = new();
        private Dictionary<string, string> _adjustParameters = new();
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct(
            DiContainer diContainer,
            IProgressService progressService,
            ISystemService systemService,
            IAttributionService attributionService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _systemService = systemService;
            _attributionService = attributionService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _campaignName.Value = _progressService.GetCampaignName();

            _globalParameters = new();
            _innerService = CreateAnalyticsInternalService(_analyticsType);

            _innerService.InitializeAsync(cancellationToken);

            InitializeDeferred();

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _isInitialized = false;
            _globalParameters.Clear();
            _adjustParameters.Clear();

            _disposables.Dispose();

            _innerService?.Release();
        }

        public void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            logEventDelayed(eventName, eventParams);
        }

        public void LogEvent(string eventName, int value)
        {
            logEventDelayed(eventName, value);
        }

        public void LogProgressionEvent(ProgressionState state, string progression01, string progression02, string progression03)
        {
            _innerService.LogProgressionEvent(state, progression01, progression02, progression03);
        }

        public void SetUserProperty(string propertyName, int value)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperty(propertyName, value);
            }
        }

        public void SetUserProperty(string propertyName, string value, bool everywhere = false)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperty(propertyName, value);

                if (everywhere)
                {
                    _globalParameters.Add(propertyName, value);
                }
            }
        }

        public void SetUserProperties(IDictionary<string, object> properties)
        {
            if (_isInitialized)
            {
                _innerService.SetUserProperties(properties);
            }
        }

        private IAnalyticsService CreateAnalyticsInternalService(AnalyticsType analyticsType)
        {
            switch (analyticsType)
            {
                #if TEL_AMPLITUDE
                case AnalyticsType.Amplitude: return new AmplitudeService(_analyticsKey, _shouldEnableLogs);
                #endif
                #if TEL_GAMEANALYTICS
                case AnalyticsType.GameAnalytics: return new GameAnalyticsService(_gameAnalyticsPrefab);
                #endif
                case AnalyticsType.AppMetrica: return new AppMetricaService(_analyticsKey, _shouldEnableLogs);
                default: throw new ArgumentOutOfRangeException("Unknown analytics type");
            }
        }

        private void InitializeDeferred()
        {
            var (isFirstLaunch, _) = PlayerPrefsUtility.GetFirstLaunch();

            var progressService = _diContainer.Resolve<IProgressService>();
            // var levelsCompleted = progressService.GetLevelProgress();
            // var cyclesCompleted = progressService.GetCyclesProgress();
            _remoteConfigName = progressService.GetRemoteConfigName();

            _innerService.SetUserProperties(new Dictionary<string, object>{
                { AnalyticsParameters.RemoteConfig, _remoteConfigName },
                // { CoreGenericAnalyticsParameters.LevelsCompleted, levelsCompleted },
                // { CoreGenericAnalyticsParameters.CyclesCompleted, cyclesCompleted },
            });

            _globalParameters.Add(AnalyticsParameters.RemoteConfig, _remoteConfigName);

            var (abTest, abGroup) = _remoteConfigName.SplitByLastOccurrence('_');
            _adjustParameters.Add(AnalyticsParameters.AdjustAbTest, abTest);
            _adjustParameters.Add(AnalyticsParameters.AdjustAbGroup, abGroup);

            _attributionService.CampaignName
                .Where(x => !string.IsNullOrEmpty(x))
                .Subscribe(OnConfigChanged)
                .AddTo(_disposables);

            if (isFirstLaunch)
            {
                _innerService.LogEvent(AnalyticsEvents.LaunchFirstTime, null);
                _progressService.SetVersion();
            }

            _systemService.OnApplicationFocusChanged.Subscribe(OnApplicationFocus).AddTo(_disposables);
            OnApplicationFocus(true); // Hack

            _isInitialized = true;
            _delayedEvents.ForEach(x => x.Invoke());
            _delayedEvents.Clear();
        }

        private void OnConfigChanged(string campaign)
        {
            _innerService.SetUserProperty(AnalyticsParameters.AdjustAttribution, campaign);
            _progressService.SetCampaignName(campaign);
            _campaignName.Value = campaign;
        }

        public void logEventDelayed(string eventName, Dictionary<string, object> parameters = null)
        {
            Action delayedEvent = () => _innerService.LogEvent(eventName, parameters);
            Action action = _isInitialized ? delayedEvent : () => _delayedEvents.Add(delayedEvent);
            action.Invoke();
        }

        public void logEventDelayed(string eventName, int value)
        {
            Action delayedEvent = () => _innerService.LogEvent(eventName, value);
            Action action = _isInitialized ? delayedEvent : () => _delayedEvents.Add(delayedEvent);
            action.Invoke();
        }

        public void FlushEvents()
        {
            _innerService.FlushEvents();
        }

        public void LogAdjustEvent(IDictionary<string, object> properties)
        {
            var callbackParams = new Dictionary<string, string>();
            properties.ForEach(pair => callbackParams[pair.Key] = pair.Value.ToString());
            _adjustParameters.ForEach(pair => callbackParams[pair.Key] = pair.Value);
            _attributionService.TrackEvent(AdjustEventToken, callbackParams);
        }

        public void SetCampaignName(string campaignName)
        {
            _progressService.SetCampaignName(campaignName);
            _campaignName.Value = campaignName;
        }

        public static void LogEventStatic(string eventName, Dictionary<string, object> details = null)
        {
            AppMetricaService.LogEventStatic(eventName, details);
        }

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
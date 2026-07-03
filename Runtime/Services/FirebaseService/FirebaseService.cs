using Firebase.Crashlytics;
using Firebase.RemoteConfig;
using Firebase.Analytics;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System;
using TapEmpire.Services;
using TapEmpire.Utility;
using System.Collections.Generic;
using Zenject;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class FirebaseService : Initializable, IFirebaseService
    {
        public static bool IsInitializedDeprecated = false;

        private ReactiveProperty<bool> _isLoaded = new(false);
        public ReadOnlyReactiveProperty<bool> IsLoaded => _isLoaded;

        public IRemoteConfiguration RemoteConfiguration { get; private set; } = null;

        private bool _isFirebaseAvailable = false;

        private IConsentService _consentService;
        private IDisposable _consentSubscription;

        [Inject]
        private void Construct(IConsentService consentService)
        {
            _consentService = consentService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _InitializeInternal(cancellationToken);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            // Fail-open: boot must continue with defaults even if Firebase is unreachable
            if (RemoteConfiguration == null)
            {
                OnConfigLoadingFinished(new EmptyRemoteConfiguration());
            }

            _consentSubscription = _consentService.IsResolved.OnceTrue(
                () => UpdateConsentStatus(_consentService.IsPersonalized.CurrentValue));
        }

        protected override void OnRelease()
        {
            _consentSubscription?.Dispose();
            base.OnRelease();
        }

        private async UniTask _InitializeInternal(CancellationToken cancellationToken)
        {
            // Initialize Firebase

            var dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
            try
            {
                // Dispose the timer with the CTS — a leaked PlayerLoopTimer would Cancel() the
                // disposed source later and throw ObjectDisposedException in the player loop.
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var timeoutTimer = cts.CancelAfterSlim(TimeSpan.FromSeconds(15));
                dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync()
                    .AsUniTask()
                    .AttachExternalCancellation(cts.Token);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FirebaseManager] CheckAndFixDependenciesAsync failed or timed out: {exception}");
            }

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                _isFirebaseAvailable = true;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                IsInitializedDeprecated = true;

                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // When this property is set to true, Crashlytics will report all
                // uncaught exceptions as fatal events. This is the recommended behavior.
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                // Set a flag here for indicating that your project is ready to use Firebase.
                // var shouldLoadConfig = Game.ProgressManager.GetShouldLoadConfig();

                // System.Action loadAction = shouldLoadConfig ? SetDefaultsAndFetch : OnConfigLoadingFinished;
                // loadAction.Invoke();

                await FetchRemoteConfig(cancellationToken);
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));

                OnConfigLoadingFinished(new EmptyRemoteConfiguration());
            }
        }

        public FirebaseRemoteConfig GetNativeConfig() => FirebaseRemoteConfig.DefaultInstance;

        public static void LogEvent(string name)
        {
            if (IsInitializedDeprecated)
            {
                FirebaseAnalytics.LogEvent(name);
            }
        }

        public static void LogEvent(string name, params Parameter[] parameters)
        {
            if (IsInitializedDeprecated)
            {
                FirebaseAnalytics.LogEvent(name, parameters);
            }
        }

        public void Crash()
        {
            throw new System.Exception("Crashlytics test exception");
        }

        public void UpdateConsentStatus(bool isPersonalized)
        {
            if (_isFirebaseAvailable)
            {
                var status = isPersonalized ? ConsentStatus.Granted : ConsentStatus.Denied;
                var consent = new Dictionary<ConsentType, ConsentStatus>()
                {
                    {ConsentType.AdStorage, status },
                    {ConsentType.AnalyticsStorage, status },
                    {ConsentType.AdUserData, status },
                    {ConsentType.AdPersonalization, status },
                };

                FirebaseAnalytics.SetConsent(consent);
            }
        }

        private async UniTask FetchRemoteConfig(CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            // stopWatch.Start();
            // await NetworkUtility.WaitNetworkAsync(cancellationToken);
            // stopWatch.Stop();
            // Debug.Log($"firebase manager WaitInternetConnection took {stopWatch.Elapsed.TotalSeconds} total seconds");

            try
            {
                stopWatch.Restart();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var timeoutTimer = cts.CancelAfterSlim(TimeSpan.FromSeconds(10));
                await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero)
                    .AsUniTask()
                    .AttachExternalCancellation(cts.Token);
                stopWatch.Stop();
                Debug.Log($"firebase manager FirebaseRemoteConfig took {stopWatch.Elapsed.TotalSeconds} total seconds");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FirebaseManager] RemoteConfig.FetchAsync incomplete: Status '{exception}'");
            }

            await ActivateRetrievedRemoteConfigValues(cancellationToken);
        }

        private async UniTask ActivateRetrievedRemoteConfigValues(CancellationToken cancellationToken)
        {
            try
            {
                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                var info = remoteConfig.Info;

                if (info.LastFetchStatus != LastFetchStatus.Success)
                {
                    Debug.LogError(
                        $"[FirebaseManager] Remote data not loaded.\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
                    // OnConfigLoadingFinished(new EmptyRemoteConfiguration());
                }
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var timeoutTimer = cts.CancelAfterSlim(TimeSpan.FromSeconds(10));
                var status = await remoteConfig.ActivateAsync()
                    .AsUniTask()
                    .AttachExternalCancellation(cts.Token);
                stopWatch.Stop();
                Debug.Log($"firebase manager remoteConfig activate took {stopWatch.Elapsed.TotalSeconds} total seconds");

                // ignore status and give firebase stored config.
                OnConfigLoadingFinished(new RemoteConfiguration(FirebaseRemoteConfig.DefaultInstance.AllValues));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FirebaseManager] Remote config activation failed: {exception}");
                OnConfigLoadingFinished(new EmptyRemoteConfiguration());
            }
        }

        private void OnConfigLoadingFinished(IRemoteConfiguration remoteConfiguration)
        {
            RemoteConfiguration = remoteConfiguration;
            _isLoaded.Value = true;

            // Debug.LogError(RemoteConfiguration.GetString("ConfigName", string.Empty));
            // Debug.LogError(RemoteConfiguration.GetString("AdsSettings", string.Empty));
        }
    }
}
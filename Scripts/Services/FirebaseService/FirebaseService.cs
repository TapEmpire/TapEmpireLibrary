using Firebase.Crashlytics;
using Firebase.RemoteConfig;
using Firebase.Analytics;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System;
using TapEmpire.Utility;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class FirebaseService : Initializable, IFirebaseService
    {
        private ReactiveProperty<bool> _isLoaded = new(false);
        public ReadOnlyReactiveProperty<bool> IsLoaded => _isLoaded;

        public IRemoteConfiguration RemoteConfiguration { get; private set; } = null;

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            await _InitializeInternal(cancellationToken);
            // return UniTask.CompletedTask;
        }

        private async UniTask _InitializeInternal(CancellationToken cancellationToken)
        {
            // Initialize Firebase

            // TODO: Catch exceptions
            var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                global::FirebaseManager.hasInitialized = true;

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

        public void Crash()
        {
            throw new System.Exception("Crashlytics test exception");
        }

        private async UniTask FetchRemoteConfig(CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            // stopWatch.Start();
            await NetworkUtility.WaitNetworkAsync(cancellationToken);
            // stopWatch.Stop();
            // Debug.Log($"firebase manager WaitInternetConnection took {stopWatch.Elapsed.TotalSeconds} total seconds");

            try
            {
                stopWatch.Restart();
                await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
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
            var status = await remoteConfig.ActivateAsync();
            stopWatch.Stop();
            Debug.Log($"firebase manager remoteConfig activate took {stopWatch.Elapsed.TotalSeconds} total seconds");

            if (status)
            {
                OnConfigLoadingFinished(new RemoteConfiguration(FirebaseRemoteConfig.DefaultInstance.AllValues));
            }
            else
            {
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
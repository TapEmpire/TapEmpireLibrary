#if TEL_CLOUD_SAVE
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.UI;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class CloudSaveService : Initializable, ICloudSaveService
    {
        [SerializeField] private CloudSaveSettings _settings;
        [SerializeReference] private ICloudSaveProvider[] _providers;

        private const string CloudSaveEnabledKey = "CloudSaveEnabledKey";
        private const string CloudSaveSeenTimestampKey = "CloudSaveSeenTimestampKey";
        private const string CloudSaveLoginDeclinedKey = "CloudSaveLoginDeclinedKey";
        private const int AppleStartupProbeMaxAttempts = 3;
        private const int AppleStartupProbeRetryDelaySeconds = 5;

        private ICloudSaveProvider _activeProvider;

        public bool IsEnabled { get; private set; }

        private IProgressService _progressService;
        private IUIService _uiService;
        private ICloudSaveSerializer<ProgressSnapshot> _serializer;

        private bool _isRestoring;
        private bool _isSaving;
        private DiContainer _container;

        [Inject]
        private void Construct(IProgressService progressService, IUIService uiService, DiContainer container)
        {
            _container = container;
            _progressService = progressService;
            _uiService = uiService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _providers ??= Array.Empty<ICloudSaveProvider>();
            _serializer = new ProgressCloudSaveSerializer(_progressService, _settings);

            var hasExplicitSetting = _progressService.BoolValuesDictionary.TryGetValue(CloudSaveEnabledKey, out var enabled, canUseDefault: false);
            IsEnabled = hasExplicitSetting && enabled;

            var loginDeclined = IsLoginDeclined();
            Debug.Log($"[CloudSave] Initializing. IsEnabled={IsEnabled}, HasExplicitSetting={hasExplicitSetting}, LoginDeclined={loginDeclined}, Providers={_providers.Length}, ExcludedKeys={_serializer.ExcludedKeysCount}");

            await InitializeProvidersAsync(cancellationToken, allowManualLogin: !loginDeclined);

            // If manual login was shown and provider still unavailable → user declined
            if (!loginDeclined && !hasExplicitSetting && _activeProvider == null)
            {
                Debug.Log("[CloudSave] Provider unavailable after manual login attempt — marking login as declined.");
                SetLoginDeclined(true);
            }

            if (!hasExplicitSetting && _activeProvider is { IsAvailable: true })
            {
                Debug.Log("[CloudSave] Provider available and no prior user preference — auto-enabling cloud saves.");
                _progressService.BoolValuesDictionary.SetValue(CloudSaveEnabledKey, true);
                IsEnabled = true;
            }
            await TryShowRestoreAsync(cancellationToken);
        }

        public async UniTask<bool> TryShowRestoreAsync(CancellationToken cancellationToken)
        {
            var probeResult = await ProbeAsync(cancellationToken);
            if (!probeResult.HasCloudData)
                return false;
            
            if (!_settings || !_settings.RestoreUIViewPrefab)
            {
                Debug.LogWarning("[CloudSave] RestoreUIViewPrefab not set in CloudSaveSettings — skipping restore UI.");
                return false;
            }
            
            var viewModel = new CloudSaveRestoreUIViewModel(_container, probeResult.CloudDataTimestampMs, probeResult.CloudSnapshot);
            var tcs = new UniTaskCompletionSource<bool>();
            viewModel.OnResult = accepted => tcs.TrySetResult(accepted);

            await _uiService.OpenViewAsync(_settings.RestoreUIViewPrefab, viewModel, cancellationToken);
            return await tcs.Task;
        }

        public async UniTask<CloudSaveProbeResult> ProbeAsync(CancellationToken cancellationToken)
        {
            Debug.Log("[CloudSave] Probing for cloud data...");

            try
            {
                if (_activeProvider == null || !_activeProvider.IsAvailable)
                {
                    Debug.Log("[CloudSave] Probe: no available provider.");
                    return CloudSaveProbeResult.NoProvider("No cloud save provider is available.");
                }

                var loadResult = await LoadStartupProbeResultAsync(cancellationToken);

                if (!loadResult.Success)
                {
                    Debug.Log($"[CloudSave] Probe: load failed — {loadResult.Message}");
                    return CloudSaveProbeResult.Failed(loadResult.Message);
                }

                if (!loadResult.HasSnapshot)
                {
                    Debug.Log("[CloudSave] Probe: no cloud data found.");
                    return CloudSaveProbeResult.NoData();
                }

                loadResult.Snapshot.LogContents("[CloudSave] Probe");

                var timestamp = loadResult.Snapshot.UpdatedAtUnixMs;
                var seenTimestamp = GetSeenTimestampMs();
                if (timestamp <= seenTimestamp)
                {
                    Debug.Log($"[CloudSave] Probe: cloud data is not newer. UpdatedAt={timestamp}, SeenAt={seenTimestamp}");
                    return CloudSaveProbeResult.NoData();
                }

                Debug.Log($"[CloudSave] Probe: newer cloud data found. UpdatedAt={timestamp}, SeenAt={seenTimestamp}");
                return CloudSaveProbeResult.DataFound(timestamp, loadResult.Snapshot);
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogException(exception);
                return CloudSaveProbeResult.Failed(exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveProbeResult.Failed(exception.Message);
            }
        }

        public async UniTask<CloudSaveOperationResult> EnableAsync(CancellationToken cancellationToken)
        {
            if (IsEnabled && _activeProvider is {IsAvailable: true})
            {
                return CloudSaveOperationResult.Completed("Cloud saves are already enabled.");
            }

            Debug.Log("[CloudSave] Enabling cloud saves.");

            // Clear declined flag — user is explicitly requesting login via Settings
            SetLoginDeclined(false);

            if (_activeProvider is not {IsAvailable: true})
            {
                await InitializeProvidersAsync(cancellationToken, allowManualLogin: true);
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                Debug.Log("[CloudSave] No available provider — enable failed.");
                return CloudSaveOperationResult.Failed("No cloud save provider is available.");
            }

            _progressService.BoolValuesDictionary.SetValue(CloudSaveEnabledKey, true);
            IsEnabled = true;
            Debug.Log("[CloudSave] Cloud saves enabled. Flag persisted.");

            return CloudSaveOperationResult.Completed("Cloud saves enabled.");
        }

        public void Disable()
        {
            Debug.Log("[CloudSave] Disabling cloud saves.");
            _progressService.BoolValuesDictionary.SetValue(CloudSaveEnabledKey, false);
            IsEnabled = false;
            _activeProvider = null;
            Debug.Log("[CloudSave] Cloud saves disabled.");
        }

        public void DeclineRestore(long cloudDataTimestampMs)
        {
            SetSeenTimestampMs(cloudDataTimestampMs);
            Debug.Log($"[CloudSave] Restore declined. Saved seen timestamp={cloudDataTimestampMs}");
        }

        private async UniTask InitializeProvidersAsync(CancellationToken cancellationToken, bool allowManualLogin = true)
        {
            _activeProvider = null;
            foreach (var provider in _providers)
            {
                if (provider == null)
                {
                    continue;
                }

                Debug.Log($"[CloudSave] Initializing provider: {provider.GetType().Name} (allowManualLogin={allowManualLogin})");
                await provider.InitializeAsync(cancellationToken, allowManualLogin);

                if (provider.IsAvailable)
                {
                    _activeProvider = provider;
                    Debug.Log($"[CloudSave] Active provider: {provider.GetType().Name}");
                    break;
                }

                Debug.Log($"[CloudSave] Provider {provider.GetType().Name} is not available, trying next.");
            }
        }
        

        public async UniTask<CloudSaveOperationResult> RestoreAsync(CancellationToken cancellationToken)
        {
            if (_isRestoring)
                return CloudSaveOperationResult.Ignored("Cloud restore already in progress.");

            try
            {
                Debug.Log("[CloudSave] Restore started (loading from provider).");
                var loadResult = await _activeProvider.LoadAsync(cancellationToken);
                Debug.Log($"[CloudSave] Load result: Success={loadResult.Success}, HasSnapshot={loadResult.HasSnapshot}, Message='{loadResult.Message}'");

                if (!loadResult.Success)
                    return CloudSaveOperationResult.Failed(loadResult.Message);

                if (!loadResult.HasSnapshot)
                {
                    Debug.Log("[CloudSave] No remote snapshot found.");
                    return CloudSaveOperationResult.Completed("Cloud snapshot not found.");
                }

                var remoteSnapshot = loadResult.Snapshot;
                var remoteTimestamp = remoteSnapshot?.UpdatedAtUnixMs ?? 0;
                var seenTimestamp = GetSeenTimestampMs();
                if (remoteTimestamp <= seenTimestamp)
                {
                    Debug.Log($"[CloudSave] Restore skipped: cloud data is not newer. UpdatedAt={remoteTimestamp}, SeenAt={seenTimestamp}");
                    return CloudSaveOperationResult.Ignored("Cloud snapshot is not newer than seen timestamp.");
                }

                return await RestoreAsync(remoteSnapshot, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
        }

        public async UniTask<CloudSaveOperationResult> RestoreAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (_isRestoring)
                return CloudSaveOperationResult.Ignored("Cloud restore already in progress.");

            _isRestoring = true;
            var result = CloudSaveOperationResult.Ignored("Cloud restore skipped.");

            try
            {
                Debug.Log($"[CloudSave] Restoring from snapshot. IntValues={snapshot?.IntValues?.Count ?? 0}, BoolValues={snapshot?.BoolValues?.Count ?? 0}, StringValues={snapshot?.StringValues?.Count ?? 0}, UpdatedAt={snapshot?.UpdatedAtUnixMs}");
                _serializer.Import(snapshot);
                SetSeenTimestampMs(snapshot?.UpdatedAtUnixMs ?? 0);
                result = CloudSaveOperationResult.Completed("Cloud snapshot restored.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result = CloudSaveOperationResult.Failed(exception.Message);
            }
            finally
            {
                _isRestoring = false;
                Debug.Log($"[CloudSave] Restore finished: Success={result.Success}, Skipped={result.Skipped}, Message='{result.Message}'");
            }

            return result;
        }

        public async UniTask<CloudSaveOperationResult> SaveAsync(CancellationToken cancellationToken)
        {
            if (_activeProvider == null)
                return CloudSaveOperationResult.Ignored("Cloud provider is null.");
            
            if (!IsEnabled)
                return CloudSaveOperationResult.Ignored("Cloud saves are not enabled.");
            
            if (_isRestoring)
            {
                Debug.Log("[CloudSave] Save skipped: restore is in progress.");
                return CloudSaveOperationResult.Ignored("Cloud restore in progress.");
            }

            if (_isSaving)
                return CloudSaveOperationResult.Ignored("Cloud save already in progress.");
            
            _isSaving = true;
            var result = CloudSaveOperationResult.Ignored("Cloud save skipped.");

            try
            {
                var snapshot = _serializer.Export();
                snapshot.LogContents("[CloudSave] Save");
                result = await _activeProvider.SaveAsync(snapshot, cancellationToken);
                if (result.Success)
                {
                    SetSeenTimestampMs(snapshot.UpdatedAtUnixMs);
                }
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogException(exception);
                result = CloudSaveOperationResult.Failed(exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result = CloudSaveOperationResult.Failed(exception.Message);
            }
            finally
            {
                _isSaving = false;
                Debug.Log($"[CloudSave] Save finished: Success={result.Success}, Skipped={result.Skipped}, Message='{result.Message}'");
            }

            return result;
        }

        public async UniTask<CloudSaveOperationResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (_isRestoring)
            {
                return CloudSaveOperationResult.Ignored("Cloud restore in progress.");
            }

            if (_isSaving)
            {
                return CloudSaveOperationResult.Ignored("Cloud save in progress.");
            }
            
            try
            {
                Debug.Log("[CloudSave] Delete started.");

                var result = await _activeProvider.DeleteAsync(cancellationToken);

                if (result.Success)
                {
                    SetSeenTimestampMs(0);
                    Debug.Log("[CloudSave] Delete completed. Seen timestamp reset.");
                }
                else
                {
                    Debug.Log($"[CloudSave] Delete finished with result: Success={result.Success}, Message='{result.Message}'");
                }

                return result;
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
        }

        private bool IsLoginDeclined()
        {
            _progressService.BoolValuesDictionary.TryGetValue(CloudSaveLoginDeclinedKey, out var declined, canUseDefault: false);
            return declined;
        }

        private async UniTask<CloudSaveLoadResult> LoadStartupProbeResultAsync(CancellationToken cancellationToken)
        {
            if (!ShouldRetryAppleStartupProbe())
            {
                return await _activeProvider.LoadAsync(cancellationToken);
            }

            CloudSaveLoadResult loadResult = default;

            for (var attempt = 1; attempt <= AppleStartupProbeMaxAttempts; attempt++)
            {
                Debug.Log($"[CloudSave][iOS] Startup probe attempt {attempt}/{AppleStartupProbeMaxAttempts}.");
                loadResult = await _activeProvider.LoadAsync(cancellationToken);

                if (!loadResult.Success)
                {
                    Debug.Log($"[CloudSave][iOS] Startup probe attempt {attempt}/{AppleStartupProbeMaxAttempts} failed: {loadResult.Message}");
                    return loadResult;
                }

                if (loadResult.HasSnapshot)
                {
                    Debug.Log($"[CloudSave][iOS] Startup probe found snapshot on attempt {attempt}/{AppleStartupProbeMaxAttempts}.");
                    return loadResult;
                }

                if (attempt < AppleStartupProbeMaxAttempts)
                {
                    Debug.Log($"[CloudSave][iOS] Startup probe attempt {attempt}/{AppleStartupProbeMaxAttempts} returned empty result. Waiting {AppleStartupProbeRetryDelaySeconds} seconds before retry.");
                    await UniTask.Delay(TimeSpan.FromSeconds(AppleStartupProbeRetryDelaySeconds), cancellationToken: cancellationToken);
                }
            }

            Debug.Log($"[CloudSave][iOS] Startup probe found no snapshot after {AppleStartupProbeMaxAttempts} attempts.");
            return loadResult;
        }

        private bool ShouldRetryAppleStartupProbe()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _activeProvider is AppleCloudSaveProvider;
#else
            return false;
#endif
        }

        private void SetLoginDeclined(bool declined)
        {
            _progressService.BoolValuesDictionary.SetValue(CloudSaveLoginDeclinedKey, declined);
        }

        private long GetSeenTimestampMs()
        {
            if (_progressService == null)
            {
                return 0;
            }

            _progressService.StringValuesDictionary.TryGetValue(CloudSaveSeenTimestampKey, out var seenTimestamp, canUseDefault: false);
            return long.TryParse(seenTimestamp, out var value) ? value : 0;
        }

        private void SetSeenTimestampMs(long timestampMs)
        {
            _progressService.StringValuesDictionary.SetValue(CloudSaveSeenTimestampKey, timestampMs.ToString());
        }
    }
}
#endif

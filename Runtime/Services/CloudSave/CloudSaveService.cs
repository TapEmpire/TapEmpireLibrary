using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
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

        private ICloudSaveProvider _activeProvider;

        public bool IsEnabled { get; private set; }
        public bool IsAvailable => IsEnabled && (_activeProvider?.IsAvailable ?? false);
        public bool IsRestored { get; private set; }

        private IProgressService _progressService;
        private ProgressSnapshotMapper _snapshotMapper;

        private bool _isRestoring;
        private bool _isSaving;

        [Inject]
        private void Construct(IProgressService progressService)
        {
            _progressService = progressService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _providers ??= Array.Empty<ICloudSaveProvider>();
            _snapshotMapper = new ProgressSnapshotMapper(_progressService, _settings);

            Debug.Log($"[CloudSave] Initializing. IsEnabled={IsEnabled}, Providers={_providers.Length}, TrackedKeys={_snapshotMapper.TrackedKeysCount}");

            SetEnabled();

            if (!IsEnabled)
            {
                Debug.Log("[CloudSave] Cloud saves are not enabled. Skipping provider initialization.");
                return;
            }

            await InitializeProvidersAsync(cancellationToken);
        }

        private void SetEnabled()
        {
            if (_progressService == null)
            {
                IsEnabled = false;
                return;
            }

            _progressService.BoolValuesDictionary.TryGetValue(CloudSaveEnabledKey, out var enabled, canUseDefault: false);
            IsEnabled = enabled;
        }

        public async UniTask<CloudSaveProbeResult> ProbeAsync(CancellationToken cancellationToken)
        {
            Debug.Log("[CloudSave] Probing for cloud data...");

            try
            {
                if (_activeProvider == null)
                {
                    await InitializeProvidersAsync(cancellationToken);
                }

                if (_activeProvider == null || !_activeProvider.IsAvailable)
                {
                    Debug.Log("[CloudSave] Probe: no available provider.");
                    return CloudSaveProbeResult.NoProvider("No cloud save provider is available.");
                }

                var loadResult = await _activeProvider.LoadAsync(cancellationToken);

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

                var timestamp = loadResult.Snapshot.UpdatedAtUnixMs;
                var seenTimestamp = GetSeenTimestampMs();
                if (timestamp <= seenTimestamp)
                {
                    Debug.Log($"[CloudSave] Probe: cloud data is not newer. UpdatedAt={timestamp}, SeenAt={seenTimestamp}");
                    return CloudSaveProbeResult.NoData();
                }

                Debug.Log($"[CloudSave] Probe: newer cloud data found. UpdatedAt={timestamp}, SeenAt={seenTimestamp}");
                return CloudSaveProbeResult.DataFound(timestamp);
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
            if (IsEnabled && _activeProvider != null && _activeProvider.IsAvailable)
            {
                return CloudSaveOperationResult.Completed("Cloud saves are already enabled.");
            }

            Debug.Log("[CloudSave] Enabling cloud saves.");

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                await InitializeProvidersAsync(cancellationToken);
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

        private async UniTask InitializeProvidersAsync(CancellationToken cancellationToken)
        {
            _activeProvider = null;
            foreach (var provider in _providers)
            {
                if (provider == null)
                {
                    continue;
                }

                Debug.Log($"[CloudSave] Initializing provider: {provider.GetType().Name}");
                await provider.InitializeAsync(cancellationToken);

                if (provider.IsAvailable)
                {
                    _activeProvider = provider;
                    Debug.Log($"[CloudSave] Active provider: {provider.GetType().Name}");
                    break;
                }

                Debug.Log($"[CloudSave] Provider {provider.GetType().Name} is not available, trying next.");
            }

            Debug.Log($"[CloudSave] Provider resolution done. IsAvailable={IsAvailable}");
        }
        

        public async UniTask<CloudSaveOperationResult> RestoreAsync(CancellationToken cancellationToken)
        {
            if (_isRestoring)
            {
                return CloudSaveOperationResult.Ignored("Cloud restore already in progress.");
            }

            if (!IsAvailable)
            {
                IsRestored = true;
                var unavailableResult = CloudSaveOperationResult.Ignored("Cloud provider is unavailable.");
                Debug.Log($"[CloudSave] Restore skipped: provider unavailable.");
                return unavailableResult;
            }

            _isRestoring = true;
            var result = CloudSaveOperationResult.Ignored("Cloud restore skipped.");

            try
            {
                Debug.Log("[CloudSave] Restore started.");
                var localSnapshot = _snapshotMapper.Export();
                Debug.Log($"[CloudSave] Local snapshot exported. IntValues={localSnapshot?.IntValues?.Count ?? 0}, BoolValues={localSnapshot?.BoolValues?.Count ?? 0}, StringValues={localSnapshot?.StringValues?.Count ?? 0}, UpdatedAt={localSnapshot?.UpdatedAtUnixMs}");
                var loadResult = await _activeProvider.LoadAsync(cancellationToken);
                Debug.Log($"[CloudSave] Load result: Success={loadResult.Success}, HasSnapshot={loadResult.HasSnapshot}, Message='{loadResult.Message}'");

                if (!loadResult.Success)
                {
                    result = CloudSaveOperationResult.Failed(loadResult.Message);
                    return result;
                }

                if (!loadResult.HasSnapshot)
                {
                    IsRestored = true;
                    Debug.Log("[CloudSave] No remote snapshot found.");
                    result = CloudSaveOperationResult.Completed("Cloud snapshot not found.");
                    return result;
                }

                var remoteSnapshot = loadResult.Snapshot;
                Debug.Log($"[CloudSave] Remote snapshot received. IntValues={remoteSnapshot?.IntValues?.Count ?? 0}, BoolValues={remoteSnapshot?.BoolValues?.Count ?? 0}, StringValues={remoteSnapshot?.StringValues?.Count ?? 0}, UpdatedAt={remoteSnapshot?.UpdatedAtUnixMs}");
                var remoteTimestamp = remoteSnapshot?.UpdatedAtUnixMs ?? 0;
                var seenTimestamp = GetSeenTimestampMs();
                if (remoteTimestamp <= seenTimestamp)
                {
                    IsRestored = true;
                    Debug.Log($"[CloudSave] Restore skipped: cloud data is not newer. UpdatedAt={remoteTimestamp}, SeenAt={seenTimestamp}");
                    result = CloudSaveOperationResult.Ignored("Cloud snapshot is not newer than seen timestamp.");
                    return result;
                }

                Debug.Log("[CloudSave] Importing remote snapshot into local progress.");
                _snapshotMapper.Import(remoteSnapshot);
                SetSeenTimestampMs(remoteTimestamp);

                IsRestored = true;
                result = CloudSaveOperationResult.Completed("Cloud snapshot restored.");
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
                _isRestoring = false;
                Debug.Log($"[CloudSave] Restore finished: Success={result.Success}, Skipped={result.Skipped}, Message='{result.Message}'");
            }

            return result;
        }

        public async UniTask<CloudSaveOperationResult> SaveAsync(CancellationToken cancellationToken)
        {
            if (_isRestoring)
            {
                var restoringResult = CloudSaveOperationResult.Ignored("Cloud restore in progress.");
                Debug.Log("[CloudSave] Save skipped: restore is in progress.");
                return restoringResult;
            }

            if (_isSaving)
            {
                return CloudSaveOperationResult.Ignored("Cloud save already in progress.");
            }

            if (!IsAvailable)
            {
                var unavailableResult = CloudSaveOperationResult.Ignored("Cloud provider is unavailable.");
                Debug.Log("[CloudSave] Save skipped: provider unavailable.");
                return unavailableResult;
            }

            _isSaving = true;
            var result = CloudSaveOperationResult.Ignored("Cloud save skipped.");

            try
            {
                var snapshot = _snapshotMapper.Export();
                Debug.Log($"[CloudSave] Save started. IntValues={snapshot?.IntValues?.Count ?? 0}, BoolValues={snapshot?.BoolValues?.Count ?? 0}, StringValues={snapshot?.StringValues?.Count ?? 0}, UpdatedAt={snapshot?.UpdatedAtUnixMs}");
                result = await _activeProvider.SaveAsync(snapshot, cancellationToken);
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

            if (!IsAvailable)
            {
                return CloudSaveOperationResult.Ignored("Cloud provider is unavailable.");
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

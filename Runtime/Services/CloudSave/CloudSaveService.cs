using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
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
        public bool HasPendingChanges => _hasPendingChanges;

        public Observable<CloudSaveOperationResult> OnRestoreFinished => _onRestoreFinished;
        public Observable<CloudSaveOperationResult> OnSaveFinished => _onSaveFinished;

        private readonly Subject<CloudSaveOperationResult> _onRestoreFinished = new();
        private readonly Subject<CloudSaveOperationResult> _onSaveFinished = new();

        private IProgressService _progressService;
        private ISystemService _systemService;

        private CompositeDisposable _disposables = new();
        private readonly Dictionary<string, CloudSaveTrackedValueType> _trackedKeyTypes = new();
        private string[] _trackedKeys = Array.Empty<string>();

        private HashSet<string> _trackedKeysSet;
        private CancellationTokenSource _debouncedSaveCancellationTokenSource;
        private bool _isRestoring;
        private bool _isSaving;
        private bool _hasPendingChanges;
        private bool _suppressDirtyTracking;

        [Inject]
        private void Construct(IProgressService progressService, ISystemService systemService)
        {
            _progressService = progressService;
            _systemService = systemService;
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _providers ??= Array.Empty<ICloudSaveProvider>();
            _disposables = new CompositeDisposable();
            BuildTrackedKeyConfiguration();

            Debug.Log($"[CloudSave] Initializing. IsEnabled={IsEnabled}, Providers={_providers.Length}, TrackedKeys={_trackedKeys.Length}");

            SubscribeProgressChanges();
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

        private void BuildTrackedKeyConfiguration()
        {
            _trackedKeyTypes.Clear();

            if (_settings?.TrackedKeyTypes is { Length: > 0 } typedKeys)
            {
                foreach (var trackedKey in typedKeys)
                {
                    if (string.IsNullOrWhiteSpace(trackedKey.Key))
                    {
                        continue;
                    }

                    _trackedKeyTypes[trackedKey.Key] = trackedKey.ValueType;
                }
            }

            var keys = new List<string>(_trackedKeyTypes.Count);
            foreach (var pair in _trackedKeyTypes)
            {
                keys.Add(pair.Key);
            }

            _trackedKeys = keys.ToArray();
            _trackedKeysSet = _trackedKeys.Length > 0 ? new HashSet<string>(_trackedKeys) : null;
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
            catch (OperationCanceledException)
            {
                throw;
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

            _hasPendingChanges = true;
            ScheduleSave();
            return CloudSaveOperationResult.Completed("Cloud saves enabled.");
        }

        public void Disable()
        {
            Debug.Log("[CloudSave] Disabling cloud saves.");
            _progressService.BoolValuesDictionary.SetValue(CloudSaveEnabledKey, false);
            IsEnabled = false;

            CancelPendingSave();
            _activeProvider = null;
            _hasPendingChanges = false;

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

            if (_settings != null && _settings.SaveOnFocusLost && _systemService != null)
            {
                _systemService.OnApplicationFocusChanged.Subscribe(OnApplicationFocusChanged).AddTo(_disposables);
            }
        }

        protected override void OnRelease()
        {
            _disposables.Dispose();
            UnsubscribeProgressChanges();
            CancelPendingSave();
            _onRestoreFinished.Dispose();
            _onSaveFinished.Dispose();
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
                _onRestoreFinished.OnNext(unavailableResult);
                return unavailableResult;
            }

            _isRestoring = true;
            var result = CloudSaveOperationResult.Ignored("Cloud restore skipped.");

            try
            {
                Debug.Log("[CloudSave] Restore started.");
                var localSnapshot = ExportSnapshot();
                Debug.Log($"[CloudSave] Local snapshot exported. IntValues={localSnapshot?.IntValues?.Count ?? 0}, BoolValues={localSnapshot?.BoolValues?.Count ?? 0}, StringValues={localSnapshot?.StringValues?.Count ?? 0}, UpdatedAt={localSnapshot?.UpdatedAtUnixMs}");
                LogSnapshotState("Local snapshot state", localSnapshot);

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

                    if (!ProgressSnapshotUtility.IsEmpty(localSnapshot))
                    {
                        Debug.Log("[CloudSave] No remote snapshot found, local data exists — scheduling upload.");
                        _hasPendingChanges = true;
                        ScheduleSave();
                    }
                    else
                    {
                        Debug.Log("[CloudSave] No remote snapshot found, local data is empty — nothing to do.");
                    }

                    result = CloudSaveOperationResult.Completed("Cloud snapshot not found.");
                    return result;
                }

                var remoteSnapshot = loadResult.Snapshot;
                Debug.Log($"[CloudSave] Remote snapshot received. IntValues={remoteSnapshot?.IntValues?.Count ?? 0}, BoolValues={remoteSnapshot?.BoolValues?.Count ?? 0}, StringValues={remoteSnapshot?.StringValues?.Count ?? 0}, UpdatedAt={remoteSnapshot?.UpdatedAtUnixMs}");
                LogSnapshotState("Remote snapshot state", remoteSnapshot);
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
                _suppressDirtyTracking = true;
                ImportSnapshot(remoteSnapshot);
                _suppressDirtyTracking = false;
                LogSnapshotState("Applied remote snapshot state", remoteSnapshot);

                SetSeenTimestampMs(remoteTimestamp);
                _hasPendingChanges = false;

                IsRestored = true;
                result = CloudSaveOperationResult.Completed("Cloud snapshot restored.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result = CloudSaveOperationResult.Failed(exception.Message);
            }
            finally
            {
                _suppressDirtyTracking = false;
                _isRestoring = false;
                Debug.Log($"[CloudSave] Restore finished: Success={result.Success}, Skipped={result.Skipped}, Message='{result.Message}'");
                _onRestoreFinished.OnNext(result);
            }

            return result;
        }

        public async UniTask<CloudSaveOperationResult> SaveAsync(CancellationToken cancellationToken)
        {
            if (_isRestoring)
            {
                var restoringResult = CloudSaveOperationResult.Ignored("Cloud restore in progress.");
                Debug.Log("[CloudSave] Save skipped: restore is in progress.");
                _onSaveFinished.OnNext(restoringResult);
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
                _onSaveFinished.OnNext(unavailableResult);
                return unavailableResult;
            }

            if (!_hasPendingChanges)
            {
                var noChangesResult = CloudSaveOperationResult.Ignored("No local changes to upload.");
                Debug.Log("[CloudSave] Save skipped: no pending changes.");
                _onSaveFinished.OnNext(noChangesResult);
                return noChangesResult;
            }

            _isSaving = true;
            var result = CloudSaveOperationResult.Ignored("Cloud save skipped.");

            try
            {
                var snapshot = ExportSnapshot();
                Debug.Log($"[CloudSave] Save started. IntValues={snapshot?.IntValues?.Count ?? 0}, BoolValues={snapshot?.BoolValues?.Count ?? 0}, StringValues={snapshot?.StringValues?.Count ?? 0}, UpdatedAt={snapshot?.UpdatedAtUnixMs}");
                LogSnapshotState("Save snapshot state", snapshot);
                result = await _activeProvider.SaveAsync(snapshot, cancellationToken);
                if (result.Success)
                {
                    _hasPendingChanges = false;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
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
                _onSaveFinished.OnNext(result);
            }

            return result;
        }

        public async UniTask<CloudSaveOperationResult> SyncAsync(CancellationToken cancellationToken)
        {
            var restoreResult = await RestoreAsync(cancellationToken);
            if (_hasPendingChanges)
            {
                var saveResult = await SaveAsync(cancellationToken);
                if (!saveResult.Success && !saveResult.Skipped)
                {
                    return saveResult;
                }
            }

            return restoreResult;
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

        #region Snapshot

        private ProgressSnapshot ExportSnapshot()
        {
            var snapshot = new ProgressSnapshot
            {
                SchemaVersion = ProgressSnapshot.CurrentSchemaVersion,
                UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeviceId = SystemInfo.deviceUniqueIdentifier,
            };

            if (_trackedKeys == null || _trackedKeys.Length == 0)
            {
                return snapshot;
            }

            foreach (var key in _trackedKeys)
            {
                switch (ResolveTrackedKeyType(key))
                {
                    case CloudSaveTrackedValueType.String:
                        if (_progressService.StringValuesDictionary.TryGetValue(key, out var stringValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = stringValue;
                        }
                        else if (_progressService.IntValuesDictionary.TryGetValue(key, out var legacyIntValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = legacyIntValue.ToString();
                        }
                        else if (_progressService.BoolValuesDictionary.TryGetValue(key, out var legacyBoolValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = legacyBoolValue.ToString();
                        }
                        break;

                    case CloudSaveTrackedValueType.Int:
                        if (_progressService.IntValuesDictionary.TryGetValue(key, out var intValue, canUseDefault: false))
                        {
                            snapshot.IntValues[key] = intValue;
                        }
                        break;

                    case CloudSaveTrackedValueType.Bool:
                        if (_progressService.BoolValuesDictionary.TryGetValue(key, out var boolValue, canUseDefault: false))
                        {
                            snapshot.BoolValues[key] = boolValue;
                        }
                        break;

                    default:
                        if (_progressService.StringValuesDictionary.TryGetValue(key, out var fallbackString, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = fallbackString;
                        }
                        else if (_progressService.IntValuesDictionary.TryGetValue(key, out var fallbackInt, canUseDefault: false))
                        {
                            snapshot.IntValues[key] = fallbackInt;
                        }
                        else if (_progressService.BoolValuesDictionary.TryGetValue(key, out var fallbackBool, canUseDefault: false))
                        {
                            snapshot.BoolValues[key] = fallbackBool;
                        }
                        break;
                }
            }

            return snapshot;
        }

        private void ImportSnapshot(ProgressSnapshot snapshot)
        {
            if (snapshot == null || _trackedKeys == null || _trackedKeys.Length == 0)
            {
                return;
            }

            var incomingIntValues = snapshot.IntValues ?? new Dictionary<string, int>();
            var incomingBoolValues = snapshot.BoolValues ?? new Dictionary<string, bool>();
            var incomingStringValues = snapshot.StringValues ?? new Dictionary<string, string>();

            foreach (var key in _trackedKeys)
            {
                switch (ResolveTrackedKeyType(key))
                {
                    case CloudSaveTrackedValueType.String:
                        if (incomingStringValues.TryGetValue(key, out var stringValue))
                        {
                            _progressService.StringValuesDictionary.SetValue(key, stringValue, save: false);
                        }
                        else if (incomingIntValues.TryGetValue(key, out var legacyIntValue))
                        {
                            _progressService.StringValuesDictionary.SetValue(key, legacyIntValue.ToString(), save: false);
                        }
                        else if (incomingBoolValues.TryGetValue(key, out var legacyBoolValue))
                        {
                            _progressService.StringValuesDictionary.SetValue(key, legacyBoolValue.ToString(), save: false);
                        }
                        else
                        {
                            _progressService.StringValuesDictionary.DeleteKey(key);
                        }
                        break;

                    case CloudSaveTrackedValueType.Int:
                        if (incomingIntValues.TryGetValue(key, out var intValue))
                        {
                            _progressService.IntValuesDictionary.SetValue(key, intValue, save: false);
                        }
                        else
                        {
                            _progressService.IntValuesDictionary.DeleteKey(key);
                        }
                        break;

                    case CloudSaveTrackedValueType.Bool:
                        if (incomingBoolValues.TryGetValue(key, out var boolValue))
                        {
                            _progressService.BoolValuesDictionary.SetValue(key, boolValue, save: false);
                        }
                        else
                        {
                            _progressService.BoolValuesDictionary.DeleteKey(key);
                        }
                        break;

                    default:
                        if (incomingStringValues.TryGetValue(key, out var fallbackString))
                        {
                            _progressService.StringValuesDictionary.SetValue(key, fallbackString, save: false);
                        }
                        else if (incomingIntValues.TryGetValue(key, out var fallbackInt))
                        {
                            _progressService.IntValuesDictionary.SetValue(key, fallbackInt, save: false);
                        }
                        else if (incomingBoolValues.TryGetValue(key, out var fallbackBool))
                        {
                            _progressService.BoolValuesDictionary.SetValue(key, fallbackBool, save: false);
                        }
                        else
                        {
                            _progressService.StringValuesDictionary.DeleteKey(key);
                            _progressService.IntValuesDictionary.DeleteKey(key);
                            _progressService.BoolValuesDictionary.DeleteKey(key);
                        }
                        break;
                }
            }

            PlayerPrefs.Save();
        }

        private CloudSaveTrackedValueType ResolveTrackedKeyType(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return CloudSaveTrackedValueType.Unknown;
            }

            if (_trackedKeyTypes.TryGetValue(key, out var valueType))
            {
                return valueType;
            }

            return CloudSaveTrackedValueType.Unknown;
        }

        #endregion

        #region Change Tracking

        private void SubscribeProgressChanges()
        {
            _progressService.IntValuesDictionary.OnSetValue += OnIntValueChanged;
            _progressService.IntValuesDictionary.OnDeleteKey += OnValueDeleted;
            _progressService.BoolValuesDictionary.OnSetValue += OnBoolValueChanged;
            _progressService.BoolValuesDictionary.OnDeleteKey += OnValueDeleted;
            _progressService.StringValuesDictionary.OnSetValue += OnStringValueChanged;
            _progressService.StringValuesDictionary.OnDeleteKey += OnValueDeleted;
            _progressService.OnClearProgress += OnProgressCleared;
        }

        private void UnsubscribeProgressChanges()
        {
            if (_progressService == null)
            {
                return;
            }

            _progressService.IntValuesDictionary.OnSetValue -= OnIntValueChanged;
            _progressService.IntValuesDictionary.OnDeleteKey -= OnValueDeleted;
            _progressService.BoolValuesDictionary.OnSetValue -= OnBoolValueChanged;
            _progressService.BoolValuesDictionary.OnDeleteKey -= OnValueDeleted;
            _progressService.StringValuesDictionary.OnSetValue -= OnStringValueChanged;
            _progressService.StringValuesDictionary.OnDeleteKey -= OnValueDeleted;
            _progressService.OnClearProgress -= OnProgressCleared;
        }

        private void OnIntValueChanged(string key, int value) => MarkKeyAsDirty(key);

        private void OnBoolValueChanged(string key, bool value) => MarkKeyAsDirty(key);

        private void OnStringValueChanged(string key, string value) => MarkKeyAsDirty(key);

        private void OnValueDeleted(string key) => MarkKeyAsDirty(key);

        private void OnProgressCleared() => MarkAsDirty();

        private void MarkKeyAsDirty(string key)
        {
            if (_trackedKeysSet != null && !_trackedKeysSet.Contains(key))
            {
                return;
            }

            MarkAsDirty();
        }

        private void MarkAsDirty()
        {
            if (_suppressDirtyTracking || !IsEnabled)
            {
                return;
            }

            _hasPendingChanges = true;
            Debug.Log("[CloudSave] Progress changed — marked as dirty, scheduling save.");
            ScheduleSave();
        }

        #endregion

        #region Save Scheduling

        private void ScheduleSave()
        {
            if (!IsAvailable || _isRestoring)
            {
                return;
            }

            CancelPendingSave();

            var debounce = _settings != null ? _settings.SaveDebounceSeconds : 1.0f;
            if (debounce <= 0.0f)
            {
                SaveAsync(CancellationToken.None).Forget();
                return;
            }

            _debouncedSaveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
            DelayedSaveAsync(_debouncedSaveCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid DelayedSaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                var debounce = _settings != null ? _settings.SaveDebounceSeconds : 1.0f;
                await UniTask.Delay(TimeSpan.FromSeconds(debounce), cancellationToken: cancellationToken);
                await SaveAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (hasFocus || _settings == null || !_settings.SaveOnFocusLost)
            {
                return;
            }

            Debug.Log("[CloudSave] App lost focus — triggering save.");
            SaveAsync(CancellationToken.None).Forget();
        }

        private void CancelPendingSave()
        {
            if (_debouncedSaveCancellationTokenSource == null)
            {
                return;
            }

            _debouncedSaveCancellationTokenSource.Cancel();
            _debouncedSaveCancellationTokenSource.Dispose();
            _debouncedSaveCancellationTokenSource = null;
        }

        private static void LogSnapshotState(string label, ProgressSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.Log($"[CloudSave] {label}: null");
                return;
            }

            var intValues = snapshot.IntValues ?? new Dictionary<string, int>();
            var boolValues = snapshot.BoolValues ?? new Dictionary<string, bool>();
            var stringValues = snapshot.StringValues ?? new Dictionary<string, string>();

            Debug.Log($"[CloudSave] {label}: IntKeys={intValues.Count}, BoolKeys={boolValues.Count}, StringKeys={stringValues.Count}");
        }

        #endregion
    }
}

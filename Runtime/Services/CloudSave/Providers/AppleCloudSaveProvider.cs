#if TEL_CLOUD_SAVE
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_IOS
using Apple.Core.Runtime;
using Apple.GameKit;
#endif

namespace TapEmpire.Services
{
    [Serializable]
    public class AppleCloudSaveProvider : ICloudSaveProvider
    {
        [SerializeField] private bool _enabled = true;

        private const string SavedGameName = "tapempire_progress";
        
        #if UNITY_IOS
        private bool _isAuthenticated;
        private long _authCompletedAtUnixMs;
        private bool _startupWarmupDone;

        private static readonly int[] StartupRetryIntervalsMs = { 500, 1200, 2700, 4500 };
        private const int StartupTransientWindowMs = 10000;

        private enum AppleLoadAttemptKind
        {
            SnapshotFound,
            EmptyList,
            NoTargetSave,
            EmptyPayload,
            Failed
        }

        private readonly struct AppleLoadAttemptResult
        {
            public AppleLoadAttemptKind Kind { get; }
            public ProgressSnapshot Snapshot { get; }
            public string Message { get; }

            public AppleLoadAttemptResult(AppleLoadAttemptKind kind, ProgressSnapshot snapshot = null, string message = null)
            {
                Kind = kind;
                Snapshot = snapshot;
                Message = message;
            }
        }
#endif

        public async UniTask InitializeAsync(CancellationToken cancellationToken, bool allowManualLogin = true)
        {
            if (!_enabled)
            {
                Debug.Log("[CloudSave][iOS] Initialization skipped: disabled.");
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                Debug.Log("[CloudSave][iOS] Authenticating with Game Center...");
                var localPlayer = await GKLocalPlayer.Authenticate().AsUniTask().AttachExternalCancellation(cancellationToken);
                _isAuthenticated = localPlayer?.IsAuthenticated ?? false;
                Debug.Log($"[CloudSave][iOS] Authentication result: IsAuthenticated={_isAuthenticated}");
                if (_isAuthenticated)
                {
                    _authCompletedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await WarmupSavedGamesAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[CloudSave][iOS] Authentication failed: {exception.Message}");
                _isAuthenticated = false;
            }
#else
            await UniTask.CompletedTask;
#endif
        }

        public bool IsAvailable
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                var authenticated = _isAuthenticated && GKLocalPlayer.Local.IsAuthenticated;
                var available = _enabled && authenticated;
                if (!available)
                {
                    Debug.Log($"[CloudSave][iOS] IsAvailable=false. Enabled={_enabled}, Authenticated={authenticated}");
                }
                return available;
#else
                return false;
#endif
            }
        }

#if UNITY_IOS
        private async UniTask WarmupSavedGamesAsync(CancellationToken cancellationToken)
        {
            if (_startupWarmupDone || !_isAuthenticated)
                return;

            try
            {
                Debug.Log("[CloudSave][iOS] Warmup fetch started.");
                var savedGames = await GKLocalPlayer.Local.FetchSavedGames()
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);

                Debug.Log($"[CloudSave][iOS] Warmup fetch completed. Count={savedGames?.Count ?? 0}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[CloudSave][iOS] Warmup fetch failed: {exception.Message}");
            }
            finally
            {
                _startupWarmupDone = true;
            }
        }

        private async UniTask<AppleLoadAttemptResult> LoadOnceInternalAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log($"[CloudSave][iOS] LoadOnce started. SaveName='{SavedGameName}'");

                var savedGames = await GKLocalPlayer.Local.FetchSavedGames().AsUniTask().AttachExternalCancellation(cancellationToken);

                if (savedGames == null || savedGames.Count == 0)
                {
                    Debug.Log("[CloudSave][iOS] LoadOnce: saved game list is empty.");
                    return new AppleLoadAttemptResult(AppleLoadAttemptKind.EmptyList);
                }

                var matching = savedGames
                    .Where(x => x != null && x.Name == SavedGameName)
                    .OrderByDescending(x => x.ModificationDate)
                    .ToList();

                if (matching.Count == 0)
                {
                    Debug.Log($"[CloudSave][iOS] LoadOnce: no saved game with name '{SavedGameName}' found.");
                    return new AppleLoadAttemptResult(AppleLoadAttemptKind.NoTargetSave);
                }

                if (matching.Count > 1)
                {
                    Debug.LogWarning($"[CloudSave][iOS] LoadOnce: found {matching.Count} saves with same name. Using newest by ModificationDate.");
                }

                var targetSave = matching[0];
                Debug.Log($"[CloudSave][iOS] LoadOnce: using save. DeviceName='{targetSave.DeviceName}', ModificationDate={targetSave.ModificationDate}");

                var nsData = await targetSave.LoadData()
                    .AsUniTask()
                    .AttachExternalCancellation(cancellationToken);

                var data = nsData?.Bytes;
                if (data == null || data.Length == 0)
                {
                    Debug.LogWarning("[CloudSave][iOS] LoadOnce: payload is empty.");
                    return new AppleLoadAttemptResult(AppleLoadAttemptKind.EmptyPayload, message: "Saved game payload is empty.");
                }

                var json = Encoding.UTF8.GetString(data);
                var snapshot = JsonConvert.DeserializeObject<ProgressSnapshot>(json);

                if (snapshot == null)
                {
                    return new AppleLoadAttemptResult(AppleLoadAttemptKind.Failed, message: "Failed to deserialize snapshot.");
                }

                Debug.Log($"[CloudSave][iOS] LoadOnce: snapshot loaded. SchemaVersion={snapshot.SchemaVersion}, UpdatedAt={snapshot.UpdatedAtUnixMs}");
                return new AppleLoadAttemptResult(AppleLoadAttemptKind.SnapshotFound, snapshot);
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogException(exception);
                return new AppleLoadAttemptResult(AppleLoadAttemptKind.Failed, message: exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return new AppleLoadAttemptResult(AppleLoadAttemptKind.Failed, message: exception.Message);
            }
        }

        public async UniTask<CloudSaveLoadResult> LoadForStartupAsync(CancellationToken cancellationToken)
        {
            if (!IsAvailable)
                return CloudSaveLoadResult.Completed(null);

            await WarmupSavedGamesAsync(cancellationToken);

            var firstAttempt = await LoadOnceInternalAsync(cancellationToken);

            switch (firstAttempt.Kind)
            {
                case AppleLoadAttemptKind.SnapshotFound:
                    return CloudSaveLoadResult.Completed(firstAttempt.Snapshot);
                case AppleLoadAttemptKind.NoTargetSave:
                    return CloudSaveLoadResult.Completed(null);
                case AppleLoadAttemptKind.EmptyPayload:
                    return CloudSaveLoadResult.Failed(firstAttempt.Message);
                case AppleLoadAttemptKind.Failed:
                    return CloudSaveLoadResult.Failed(firstAttempt.Message);
            }

            for (var i = 0; i < StartupRetryIntervalsMs.Length; i++)
            {
                var elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _authCompletedAtUnixMs;
                var withinTransientWindow = elapsedMs < StartupTransientWindowMs;

                if (!withinTransientWindow)
                {
                    Debug.Log($"[CloudSave][iOS] Startup load: empty list is considered final. ElapsedSinceAuthMs={elapsedMs}");
                    return CloudSaveLoadResult.Completed(null);
                }

                var delayMs = StartupRetryIntervalsMs[i];
                Debug.Log($"[CloudSave][iOS] Startup load: empty list after auth. Retry in {delayMs} ms. Attempt={i + 1}/{StartupRetryIntervalsMs.Length}");
                await UniTask.Delay(delayMs, cancellationToken: cancellationToken);

                var retryAttempt = await LoadOnceInternalAsync(cancellationToken);

                switch (retryAttempt.Kind)
                {
                    case AppleLoadAttemptKind.SnapshotFound:
                        return CloudSaveLoadResult.Completed(retryAttempt.Snapshot);
                    case AppleLoadAttemptKind.NoTargetSave:
                        return CloudSaveLoadResult.Completed(null);
                    case AppleLoadAttemptKind.EmptyPayload:
                        return CloudSaveLoadResult.Failed(retryAttempt.Message);
                    case AppleLoadAttemptKind.Failed:
                        return CloudSaveLoadResult.Failed(retryAttempt.Message);
                }

                // still EmptyList -> continue
            }

            Debug.Log("[CloudSave][iOS] Startup load: no saved games found within startup retry window.");
            return CloudSaveLoadResult.Completed(null);
        }
#endif

        public async UniTask<CloudSaveLoadResult> LoadAsync(CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveLoadResult.Completed(null);
            }

#if UNITY_IOS && !UNITY_EDITOR
            var attempt = await LoadOnceInternalAsync(cancellationToken);

            return attempt.Kind switch
            {
                AppleLoadAttemptKind.SnapshotFound => CloudSaveLoadResult.Completed(attempt.Snapshot),
                AppleLoadAttemptKind.EmptyPayload => CloudSaveLoadResult.Failed(attempt.Message),
                AppleLoadAttemptKind.Failed => CloudSaveLoadResult.Failed(attempt.Message),
                _ => CloudSaveLoadResult.Completed(null)
            };
#else
            return CloudSaveLoadResult.Completed(null);
#endif
        }

        public async UniTask<CloudSaveOperationResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveOperationResult.Ignored("iOS cloud provider is unavailable.");
            }

#if UNITY_IOS
            try
            {
                Debug.Log($"[CloudSave][iOS] Delete started. SaveName='{SavedGameName}'");
                await GKLocalPlayer.Local.DeleteSavedGames(SavedGameName).AsUniTask().AttachExternalCancellation(cancellationToken);
                Debug.Log("[CloudSave][iOS] Delete completed.");
                return CloudSaveOperationResult.Completed("iOS cloud save deleted.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
#else
            return CloudSaveOperationResult.Ignored("iOS cloud provider is unavailable.");
#endif
        }

        public async UniTask<CloudSaveOperationResult> SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveOperationResult.Ignored("iOS cloud provider is unavailable.");
            }

#if UNITY_IOS
            try
            {
                Debug.Log($"[CloudSave][iOS] Save started. SaveName='{SavedGameName}'");

                var json = JsonConvert.SerializeObject(snapshot ?? new ProgressSnapshot());
                var data = Encoding.UTF8.GetBytes(json);
                Debug.Log($"[CloudSave][iOS] Serialized snapshot, DataLength={data.Length} bytes");

                var nsData = new NSData(data);
                var savedGame = await GKLocalPlayer.Local.SaveGameData(nsData, SavedGameName).AsUniTask().AttachExternalCancellation(cancellationToken);

                Debug.Log($"[CloudSave][iOS] Save completed. Name='{savedGame?.Name}'");
                return CloudSaveOperationResult.Completed("iOS cloud save completed.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
#else
            return CloudSaveOperationResult.Ignored("iOS cloud provider is unavailable.");
#endif
        }
    }
}
#endif
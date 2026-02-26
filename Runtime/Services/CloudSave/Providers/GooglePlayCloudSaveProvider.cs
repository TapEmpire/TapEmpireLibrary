using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

namespace TapEmpire.Services
{
    [Serializable]
    public class GooglePlayCloudSaveProvider : ICloudSaveProvider
    {
        [SerializeField] private bool _enabled = true;
        
        private const string SavedGameFilename = "tapempire_progress";

#if UNITY_ANDROID
        private SignInStatus _signInStatus = SignInStatus.Canceled;
#endif

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (!_enabled)
            {
                Debug.Log("[CloudSave][Android] Initialization skipped: disabled.");
                return;
            }

#if UNITY_ANDROID
            PlayGamesPlatform.DebugLogEnabled = true;
            Debug.Log("[CloudSave][Android] Activating PlayGamesPlatform.");
            PlayGamesPlatform.Activate();

            // As per Google docs: call Authenticate once, handle result in callback.
            // https://developer.android.com/games/pgs/unity/unity-start
            Debug.Log("[CloudSave][Android] Calling PlayGamesPlatform.Instance.Authenticate...");
            var tcs = new UniTaskCompletionSource<SignInStatus>();
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                Debug.Log($"[CloudSave][Android] Authenticate callback: {status}");
                tcs.TrySetResult(status);
            });

            Debug.Log("[CloudSave][Android] Authenticate called, waiting for callback...");

            // Wait for the callback, yielding frames so PlayGamesHelperObject.Update() can pump.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));
            try
            {
                await UniTask.WaitUntil(
                    () => tcs.Task.Status != UniTaskStatus.Pending,
                    cancellationToken: timeoutCts.Token);
                _signInStatus = await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[CloudSave][Android] Authenticate timed out (15s).");
                _signInStatus = SignInStatus.Canceled;
            }

            Debug.Log($"[CloudSave][Android] Authentication result: {_signInStatus}");

            // If auto sign-in failed, try manual (shows system UI)
            if (_signInStatus != SignInStatus.Success)
            {
                Debug.Log("[CloudSave][Android] Trying ManuallyAuthenticate...");
                var manualTcs = new UniTaskCompletionSource<SignInStatus>();
                PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
                {
                    Debug.Log($"[CloudSave][Android] ManuallyAuthenticate callback: {status}");
                    manualTcs.TrySetResult(status);
                });

                using var manualTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                manualTimeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
                try
                {
                    await UniTask.WaitUntil(
                        () => manualTcs.Task.Status != UniTaskStatus.Pending,
                        cancellationToken: manualTimeoutCts.Token);
                    _signInStatus = await manualTcs.Task;
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning("[CloudSave][Android] ManuallyAuthenticate timed out (30s).");
                    _signInStatus = SignInStatus.Canceled;
                }

                Debug.Log($"[CloudSave][Android] Manual authentication result: {_signInStatus}");
            }
#else
            await UniTask.CompletedTask;
#endif
        }

        public bool IsAvailable
        {
            get
            {
#if UNITY_ANDROID
                var authenticated = PlayGamesPlatform.Instance.IsAuthenticated();
                var available = _enabled && authenticated;
                if (!available)
                {
                    Debug.Log($"[CloudSave][Android] IsAvailable=false. Enabled={_enabled}, Authenticated={authenticated}");
                }
                return available;
#else
                return false;
#endif
            }
        }

        public async UniTask<CloudSaveLoadResult> LoadAsync(CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveLoadResult.Completed(null);
            }

#if UNITY_ANDROID
            try
            {
                Debug.Log($"[CloudSave][Android] Load started. Filename='{SavedGameFilename}'");
                var savedGameClient = PlayGamesPlatform.Instance.SavedGame;

                var (openStatus, metadata) = await OpenSavedGameAsync(savedGameClient);
                Debug.Log($"[CloudSave][Android] Open result: {openStatus}, MetadataIsOpen={metadata?.IsOpen}");
                if (openStatus != SavedGameRequestStatus.Success || metadata == null)
                {
                    return CloudSaveLoadResult.Failed($"Failed to open saved game: {openStatus}");
                }

                var (readStatus, data) = await ReadBinaryDataAsync(savedGameClient, metadata);
                Debug.Log($"[CloudSave][Android] Read result: {readStatus}, DataLength={data?.Length ?? 0}");
                if (readStatus != SavedGameRequestStatus.Success)
                {
                    return CloudSaveLoadResult.Failed($"Failed to read saved game: {readStatus}");
                }

                if (data == null || data.Length == 0)
                {
                    Debug.Log("[CloudSave][Android] Empty data — no remote snapshot.");
                    return CloudSaveLoadResult.Completed(null);
                }

                var json = Encoding.UTF8.GetString(data);
                var snapshot = JsonConvert.DeserializeObject<ProgressSnapshot>(json);
                Debug.Log($"[CloudSave][Android] Snapshot deserialized. SchemaVersion={snapshot?.SchemaVersion}, UpdatedAt={snapshot?.UpdatedAtUnixMs}");
                return CloudSaveLoadResult.Completed(snapshot);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveLoadResult.Failed(exception.Message);
            }
#else
            return CloudSaveLoadResult.Completed(null);
#endif
        }

        public async UniTask<CloudSaveOperationResult> SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveOperationResult.Ignored("Android cloud provider is unavailable.");
            }

#if UNITY_ANDROID
            try
            {
                Debug.Log($"[CloudSave][Android] Save started. Filename='{SavedGameFilename}'");
                var savedGameClient = PlayGamesPlatform.Instance.SavedGame;

                var (openStatus, metadata) = await OpenSavedGameAsync(savedGameClient);
                Debug.Log($"[CloudSave][Android] Open result: {openStatus}, MetadataIsOpen={metadata?.IsOpen}");
                if (openStatus != SavedGameRequestStatus.Success || metadata == null)
                {
                    return CloudSaveOperationResult.Failed($"Failed to open saved game for writing: {openStatus}");
                }

                var json = JsonConvert.SerializeObject(snapshot ?? new ProgressSnapshot());
                var data = Encoding.UTF8.GetBytes(json);
                Debug.Log($"[CloudSave][Android] Serialized snapshot, DataLength={data.Length} bytes");

                var metadataUpdate = new SavedGameMetadataUpdate.Builder()
                    .WithUpdatedDescription($"Updated at {DateTime.UtcNow:u}")
                    .Build();

                var (commitStatus, _) = await CommitUpdateAsync(savedGameClient, metadata, metadataUpdate, data);
                Debug.Log($"[CloudSave][Android] Commit result: {commitStatus}");
                return commitStatus == SavedGameRequestStatus.Success
                    ? CloudSaveOperationResult.Completed("Android cloud save completed.")
                    : CloudSaveOperationResult.Failed($"Failed to commit saved game: {commitStatus}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CloudSaveOperationResult.Failed(exception.Message);
            }
#else
            return CloudSaveOperationResult.Ignored("Android cloud provider is unavailable.");
#endif
        }

#if UNITY_ANDROID
        private async UniTask<(SavedGameRequestStatus, ISavedGameMetadata)> OpenSavedGameAsync(ISavedGameClient client)
        {
            var tcs = new UniTaskCompletionSource<(SavedGameRequestStatus, ISavedGameMetadata)>();
            client.OpenWithAutomaticConflictResolution(
                SavedGameFilename,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseMostRecentlySaved,
                (status, metadata) => tcs.TrySetResult((status, metadata))
            );
            await UniTask.WaitUntil(() => tcs.Task.Status != UniTaskStatus.Pending);
            return await tcs.Task;
        }

        private async UniTask<(SavedGameRequestStatus, byte[])> ReadBinaryDataAsync(ISavedGameClient client, ISavedGameMetadata metadata)
        {
            var tcs = new UniTaskCompletionSource<(SavedGameRequestStatus, byte[])>();
            client.ReadBinaryData(metadata, (status, data) => tcs.TrySetResult((status, data)));
            await UniTask.WaitUntil(() => tcs.Task.Status != UniTaskStatus.Pending);
            return await tcs.Task;
        }

        private async UniTask<(SavedGameRequestStatus, ISavedGameMetadata)> CommitUpdateAsync(
            ISavedGameClient client, ISavedGameMetadata metadata,
            SavedGameMetadataUpdate update, byte[] data)
        {
            var tcs = new UniTaskCompletionSource<(SavedGameRequestStatus, ISavedGameMetadata)>();
            client.CommitUpdate(metadata, update, data, (status, resultMetadata) => tcs.TrySetResult((status, resultMetadata)));
            await UniTask.WaitUntil(() => tcs.Task.Status != UniTaskStatus.Pending);
            return await tcs.Task;
        }
#endif
    }
}

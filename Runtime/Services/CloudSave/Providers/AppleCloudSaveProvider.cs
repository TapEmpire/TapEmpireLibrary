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

        public async UniTask<CloudSaveLoadResult> LoadAsync(CancellationToken cancellationToken)
        {
            if (!IsAvailable)
            {
                return CloudSaveLoadResult.Completed(null);
            }

#if UNITY_IOS
            try
            {
                Debug.Log($"[CloudSave][iOS] Load started. SaveName='{SavedGameName}'");

                var savedGames = await GKLocalPlayer.Local.FetchSavedGames().AsUniTask().AttachExternalCancellation(cancellationToken);

                if (savedGames == null || savedGames.Count == 0)
                {
                    Debug.Log("[CloudSave][iOS] No saved games found.");
                    return CloudSaveLoadResult.Completed(null);
                }

                GKSavedGame targetSave = null;
                for (var i = 0; i < savedGames.Count; i++)
                {
                    var sg = savedGames[i];
                    if (sg.Name == SavedGameName)
                    {
                        targetSave = sg;
                        break;
                    }
                }

                if (targetSave == null)
                {
                    Debug.Log($"[CloudSave][iOS] No saved game with name '{SavedGameName}' found.");
                    return CloudSaveLoadResult.Completed(null);
                }

                Debug.Log($"[CloudSave][iOS] Found saved game. DeviceName='{targetSave.DeviceName}', ModificationDate={targetSave.ModificationDate}");

                var nsData = await targetSave.LoadData().AsUniTask().AttachExternalCancellation(cancellationToken);
                var data = nsData?.Bytes;

                if (data == null || data.Length == 0)
                {
                    Debug.Log("[CloudSave][iOS] Empty data — no remote snapshot.");
                    return CloudSaveLoadResult.Completed(null);
                }

                var json = Encoding.UTF8.GetString(data);
                var snapshot = JsonConvert.DeserializeObject<ProgressSnapshot>(json);
                Debug.Log($"[CloudSave][iOS] Snapshot deserialized. SchemaVersion={snapshot?.SchemaVersion}, UpdatedAt={snapshot?.UpdatedAtUnixMs}");
                return CloudSaveLoadResult.Completed(snapshot);
            }
            catch (OperationCanceledException)
            {
                throw;
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
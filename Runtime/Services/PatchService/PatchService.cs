using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Io.AppMetrica;
using R3;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public abstract class PatchService<T> : Initializable, IPatchService
        where T : PatchEntryBase
    {
        public const string DeviceIdProgressKey = "AppMetricaDeviceId";
        public const string UuidProgressKey = "AppMetricaUuid";
        public const string DeviceIdHashProgressKey = "AppMetricaDeviceIdHash";

        private readonly Subject<Unit> _idsUpdated = new();
        public Observable<Unit> IdsUpdated => _idsUpdated;
        
        private const string PatchVersionProgressKey = "PlayerPatchVersion";
        private const int RequestTimeoutMs = 5000;

        protected IProgressService _progressService;
#if TEL_CLOUD_SAVE
        private ICloudSaveService _cloudSaveService;
#endif

        [Inject]
        private void Construct(IProgressService progressService
#if TEL_CLOUD_SAVE
            , ICloudSaveService cloudSaveService
#endif
            )
        {
            _progressService = progressService;
#if TEL_CLOUD_SAVE
            _cloudSaveService = cloudSaveService;
#endif
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (Application.isEditor)
            {
                Debug.Log("[PlayerPatch] Skip in editor.");
                return;   
            }

            var patches = GetPatchEntries();
            if (patches == null || patches.Count == 0)
            {
                Debug.Log("[PlayerPatch] No patch entries. Fetching IDs in background.");
                FetchIdsInBackgroundAsync(CancellationToken.None).Forget();
                return;
            }

            var (deviceId, uuid, deviceIdHash) = await GetPlayerIdAsync(cancellationToken);
            _idsUpdated.OnNext(Unit.Default);

            var entry = patches.FirstOrDefault(p => MatchesPlayer(p, deviceId, uuid, deviceIdHash));
            if (entry == null)
            {
                Debug.Log($"[PlayerPatch] No matching patch for DeviceId={deviceId}, UUID={uuid}. Skipping.");
                return;
            }

            var savedPatchVersion = _progressService.GetInt(PatchVersionProgressKey);
            if (entry.Version <= savedPatchVersion)
            {
                Debug.Log($"[PlayerPatch] Patch v{entry.Version} already applied (saved={savedPatchVersion}). Skipping.");
                return;
            }

            Debug.Log($"[PlayerPatch] Applying patch v{entry.Version} for DeviceId={deviceId}, UUID={uuid}");
            ApplyPatch(entry);

            _progressService.SetInt(PatchVersionProgressKey, entry.Version);

#if TEL_CLOUD_SAVE
            await _cloudSaveService.SaveAsync(cancellationToken);
            Debug.Log("[PlayerPatch] Patch applied and cloud save forced.");
#endif
        }
        
        private async UniTaskVoid FetchIdsInBackgroundAsync(CancellationToken cancellationToken)
        {
            await GetPlayerIdAsync(cancellationToken);
            _idsUpdated.OnNext(Unit.Default);
        }

        private async UniTask<(string deviceId, string uuid, string deviceIdHash)> GetPlayerIdAsync(CancellationToken cancellationToken)
        {
            var (deviceId, uuid, deviceIdHash) = await RequestIdsAsync(cancellationToken);

            if (string.IsNullOrEmpty(deviceId) && string.IsNullOrEmpty(uuid))
            {
                Debug.Log("[PlayerPatch] Could not obtain AppMetrica IDs. Skipping.");
                return (deviceId, uuid, deviceIdHash);
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                _progressService.SetString(DeviceIdProgressKey, deviceId);
            }

            if (!string.IsNullOrEmpty(uuid))
            {
                _progressService.SetString(UuidProgressKey, uuid);
            }

            if (!string.IsNullOrEmpty(deviceIdHash))
            {
                _progressService.SetString(DeviceIdHashProgressKey, deviceIdHash);
            }
            return (deviceId, uuid, deviceIdHash);
        }

        protected abstract IReadOnlyList<T> GetPatchEntries();

        protected abstract void ApplyPatch(T entry);

        private static bool MatchesPlayer(PatchEntryBase entry, string deviceId, string uuid, string deviceIdHash)
        {
            if (!string.IsNullOrEmpty(entry.DeviceId) && !string.IsNullOrEmpty(deviceId) &&
                string.Equals(entry.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(entry.Uuid) && !string.IsNullOrEmpty(uuid) &&
                string.Equals(entry.Uuid, uuid, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            if (!string.IsNullOrEmpty(entry.DeviceIdHash) && !string.IsNullOrEmpty(deviceIdHash) &&
                string.Equals(entry.DeviceIdHash, deviceIdHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private async UniTask<(string deviceId, string uuid, string deviceIdHash)> RequestIdsAsync(CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource<(string, string, string)>();

            AppMetrica.RequestStartupParams(
                (result, error) =>
                {
                    tcs.TrySetResult((result?.DeviceId, result?.Uuid, result?.DeviceIdHash));
                },
                new[] { StartupParamsKey.AppMetricaDeviceID, StartupParamsKey.AppMetricaUuid, StartupParamsKey.AppMetricaDeviceIDHash }
            );

            bool hasResult;
            (string deviceId, string uuid, string deviceIdHash) ids;
            (hasResult, ids) = await UniTask.WhenAny(
                tcs.Task,
                UniTask.Delay(RequestTimeoutMs, cancellationToken: cancellationToken)
            );
            
            await UniTask.SwitchToMainThread(cancellationToken);

            if (hasResult)
            {
                Debug.Log($"[PlayerPatch] Got AppMetrica IDs: DeviceId={ids.deviceId}, UUID={ids.uuid},  DeviceIdHash={ids.deviceIdHash} ");
                return ids;
            }

            Debug.LogWarning("[PlayerPatch] AppMetrica ID request timed out.");
            return (null, null, null);
        }
    }
}

using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Services
{
    public interface ICloudSaveService : IService
    {
        bool IsEnabled { get; }
        bool IsAvailable { get; }
        bool IsRestored { get; }
        bool HasPendingChanges { get; }

        Observable<CloudSaveOperationResult> OnRestoreFinished { get; }
        Observable<CloudSaveOperationResult> OnSaveFinished { get; }

        UniTask<CloudSaveProbeResult> ProbeAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> EnableAsync(CancellationToken cancellationToken);
        void Disable();
        void DeclineRestore(long cloudDataTimestampMs);

        UniTask<CloudSaveOperationResult> RestoreAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> SaveAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> SyncAsync(CancellationToken cancellationToken);
    }
}

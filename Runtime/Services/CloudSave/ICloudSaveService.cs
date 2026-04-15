#if TEL_CLOUD_SAVE
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface ICloudSaveService : IService
    {
        bool IsEnabled { get; }

        UniTask<CloudSaveProbeResult> ProbeAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> EnableAsync(CancellationToken cancellationToken);
        void Disable();
        void DeclineRestore(long cloudDataTimestampMs);

        UniTask<CloudSaveOperationResult> RestoreAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> RestoreAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> SaveAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
#endif

#if TEL_CLOUD_SAVE
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface ICloudSaveProvider
    {
        bool IsAvailable { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken, bool allowManualLogin = true);
        UniTask<CloudSaveLoadResult> LoadAsync(CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> SaveAsync(ProgressSnapshot snapshot, CancellationToken cancellationToken);
        UniTask<CloudSaveOperationResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
#endif

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TapEmpire.LiveOps.UI;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOpsService : IService
    {
        LiveOpsSettings Settings { get; }

        IReadOnlyList<ILiveOps> LiveOps { get; }

        T GetLiveOps<T>() where T : ILiveOps;

        UniTask UpdateLiveOps(LiveOpsIconLayout layout = null, bool debug = false, bool forceVisualDelay = false);
    }
}

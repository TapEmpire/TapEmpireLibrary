using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOpsService : IService
    {
        LiveOpsSettings Settings { get; }

        T GetLiveOps<T>() where T : ILiveOps;

        UniTask UpdateLiveOps(List<Transform> placements = null, bool debug = false);
    }
}

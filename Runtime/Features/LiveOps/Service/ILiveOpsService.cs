using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOpsService : IService
    {
        // Subject<(OfferType OfferType, bool Autoshown, string Placement)> OnOfferShown { get; }

        LiveOpsSettings Settings { get; }

        T GetLiveOps<T>() where T : ILiveOps;
        List<ILiveOps> GetLiveOps();

        UniTask UpdateLiveOps(bool debug = false);
    }
}

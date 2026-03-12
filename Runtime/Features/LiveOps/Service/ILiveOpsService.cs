using System;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Patterns.Strategy;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOpsService : IService
    {
        // Subject<(OfferType OfferType, bool Autoshown, string Placement)> OnOfferShown { get; }

        LiveOpsSettings Settings { get; }

        T GetLiveOps<T>() where T : ILiveOps;

        UniTask UpdateLiveOps();
    }
}

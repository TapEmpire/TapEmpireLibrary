using System;
using R3;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    public interface IAttributionService : IService
    {
        ReadOnlyReactiveProperty<string> CampaignName { get; }

        void GetAdjustInfo(Action<string, string> callback);
    }
}

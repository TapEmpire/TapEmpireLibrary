using System;
using R3;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    public interface IAttributionService : IService
    {
        ReadOnlyReactiveProperty<string> CampaignName { get; }
        string AppToken { get; }

        void GetAdid(Action<string> callback);
    }
}

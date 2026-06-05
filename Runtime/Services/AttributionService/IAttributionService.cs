using R3;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    public interface IAttributionService : IService
    {
        ReadOnlyReactiveProperty<string> CampaignName { get; }
    }
}

using R3;
using TapEmpire.Services;

namespace TapEmpire.Experimental
{
    public interface IAttributionService : IService
    {
        ReadOnlyReactiveProperty<string> CampaignName { get; }
    }
}

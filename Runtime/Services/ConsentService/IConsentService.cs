using R3;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    public interface IConsentService : IService
    {
        ReadOnlyReactiveProperty<bool> IsResolved { get; }
        ReadOnlyReactiveProperty<bool> IsPersonalized { get; }
        ReadOnlyReactiveProperty<bool> IsEurArea { get; }
        bool IsForFamily { get; }
    }
}

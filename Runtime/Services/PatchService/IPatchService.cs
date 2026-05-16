using R3;

namespace TapEmpire.Services
{
    public interface IPatchService : IService
    {
        Observable<Unit> IdsUpdated { get; }
    }
}

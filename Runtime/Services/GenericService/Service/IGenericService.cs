using TapEmpire.Modules;

namespace TapEmpire.Services.Generic
{
    public interface IGenericService : IService
    {
        T GetServiceModule<T>() where T : class, IGenericServiceModule;
    }
}
using R3;

namespace TapEmpire.Services
{
    public interface IResourcesService<T> : IService
    {   
        Observable<(T, int, string)> OnResourceAdded { get; }
        Observable<(T, int, string)> OnResourceUsed { get; }
        Observable<T> OnVirtualAdded { get; } // For various events

        void Add(T resource, int amount, string reason = "");
        void Subtract(T resource, int amount, string reason = "");
        bool HasAmount(T resource, int amount);

        ResourceRuntimeData<T> GetResourceData(T type);

        void AddVirtual(T resource, int amount, string reason);
    }
}
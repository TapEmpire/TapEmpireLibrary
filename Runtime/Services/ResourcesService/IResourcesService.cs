using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IResourcesService<T> : IService
    {   
        Observable<(T, int, string)> OnResourceAdded { get; }
        Observable<(T ResourceType, int AmountLeft, string Reason)> OnResourceUsed { get; }
        Observable<T> OnVirtualAdded { get; } // For various events

        void Add(T resource, int amount, string reason = "");
        void Subtract(T resource, int amount, string reason = "");
        bool HasAmount(T resource, int amount);

        void Set(T resource, int amount); // For update purposes.

        ResourceRuntimeData<T> GetResourceData(T type);
        Sprite GetFlyingSprite(T type);

        void AddVirtual(T resource, int amount, string reason);
    }
}
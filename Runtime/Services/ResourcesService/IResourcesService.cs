using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IResourcesService<T> : IService
    {   
        Observable<(T, int, string)> OnResourceAdded { get; }
        Observable<(T ResourceType, int AmountLeft, string Reason)> OnResourceUsed { get; }
        Observable<T> OnVirtualAdded { get; } // For various events
        Observable<(T resourceType, int amountAdd, ResourceAcquireType type, string source)> OnResourceAddedDetailed { get; }
        Observable<(T resourceType, int amountUsed, ResourceAcquireType type, string source)> OnResourceUsedDetailed { get; }

        void Add(T resource, int amount, string reason = "", ResourceAcquireType acquireType = ResourceAcquireType.Free );
        void Subtract(T resource, int amount, string reason = "", ResourceAcquireType acquireType = ResourceAcquireType.Free);
        bool HasAmount(T resource, int amount);

        void Set(T resource, int amount); // For update purposes.

        ResourceRuntimeData<T> GetResourceData(T type);
        Sprite GetFlyingSprite(T type);

        void AddVirtual(T resource, int amount, string reason);
        void Refresh();
    }
}
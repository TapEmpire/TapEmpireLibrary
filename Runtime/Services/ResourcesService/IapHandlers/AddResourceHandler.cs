using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class AddResourceHandler<ResourceType> : BaseAddResourceHandler<ResourceType, AddResourceProduct<ResourceType>> { }

    [Serializable]
    public class BaseAddResourceHandler<ResourceType, T> : BaseIapHandler<T>
        where T : AddResourceProduct<ResourceType>
    {
        protected IResourcesService<ResourceType> _resourcesService;

        public override void Initialize(DiContainer diContainer)
        {
            _resourcesService = diContainer.Resolve<IResourcesService<ResourceType>>();
        }

        public override UniTask Handle(T product)
        {
            Debug.Log($"IAP AddHintsHandler Handle: {product.ProductId}");
            _resourcesService.Add(product.ResourceType, product.Amount, "ShopPaid");
            return UniTask.CompletedTask;
        }
    }
}
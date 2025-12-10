using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class AddResourceHandler<ResourceType> : BaseIapHandler<AddResourceProduct<ResourceType>>
    {
        protected IResourcesService<ResourceType> _resourcesService;

        public override void Initialize(DiContainer diContainer)
        {
            _resourcesService = diContainer.Resolve<IResourcesService<ResourceType>>();
        }

        public override UniTask Handle(AddResourceProduct<ResourceType> product)
        {
            Debug.Log($"IAP AddHintsHandler Handle: {product.ProductId}");
            _resourcesService.Add(product.ResourceType, product.Amount, Shop.ResourceUsageType.ShopPaid.ToString());
            return UniTask.CompletedTask;
        }
    }
}
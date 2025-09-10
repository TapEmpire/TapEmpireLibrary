using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class AddResourceHandler<ResourceType> : BaseIapHandler<AddResourceProduct<ResourceType>>
    {
        private IResourcesService<ResourceType> _resourcesService;

        public override bool IsConsumable => true;

        public override void Initialize(DiContainer diContainer)
        {
            _resourcesService = diContainer.Resolve<IResourcesService<ResourceType>>();
        }

        public override UniTask Handle(AddResourceProduct<ResourceType> product)
        {
            Debug.Log($"IAP AddHintsHandler Handle: {product.ProductId}");
            _resourcesService.Add(product.ResourceType, product.Amount, "ShopPaid");
            return UniTask.CompletedTask;
        }
    }
}
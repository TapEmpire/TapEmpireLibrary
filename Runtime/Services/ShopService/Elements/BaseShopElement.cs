using DG.Tweening;
using R3;
using TapEmpire.UI;
using UnityEngine;
using System.Linq;

namespace TapEmpire.Services.Shop
{
    public class BaseShopElement : MonoBehaviour
    {
        public Subject<BaseShopElement> OnShouldDestroy = new();
        public float Height => GetComponent<RectTransform>().rect.height;

        protected IUIService _uiService;
        protected IIapService _iapService;

        protected CompositeDisposable _disposables = new();

        public virtual void Initialize(ProductData data)
        {
        }

        public virtual void Initialize(OfferData data)
        {
        }

        protected virtual void OnDestroy()
        {
            _disposables.Dispose();
        }

        protected string GetPrice(string key)
        {
            var product = _iapService.GetProductInfo(key);
            return product.metadata.localizedPriceString;
        }

        protected T GetProduct<T>(string key) where T : class
        {
            var offer = _iapService.GetOfferInfoById(key);
            var product = offer.Products.FirstOrDefault(product => product is T);

            return product as T;
        }

        protected int GetReward<T>(string key)
        {
            var product = GetProduct<AddResourceProduct<T>>(key);
            return product == null ? 0 : product.Amount;
        }
    }

    public class BaseShopElement<ResourceType> : BaseShopElement
    {
        protected IResourcesService<ResourceType> _resourcesService;
        protected IAnimationService<ResourceType> _animationService;

        protected virtual Sequence AcquireResources(ResourceType resourceType, int amount, string usageType,
            Vector3 startPosition, bool shouldAddResource, ResourceAcquireType acquireType = ResourceAcquireType.Free)
        {
            var animation = _animationService.CollectResource(resourceType, amount, startPosition, shouldAddResource);

            var reason = shouldAddResource ? usageType : string.Empty;
            _resourcesService.AddVirtual(resourceType, amount, reason, acquireType);

            return animation.Play();
        }
    }
}

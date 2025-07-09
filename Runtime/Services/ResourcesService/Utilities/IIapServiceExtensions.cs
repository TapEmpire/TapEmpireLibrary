using System.Collections.Generic;
using System.Linq;
using TapEmpire.Utility;
using TMPro;

namespace TapEmpire.Services
{
    public static class IIapServiceExtensions
    {
        public static int GetReward<ResourceType>(this IIapService service, string key)
        {
            var offer = service.GetOfferInfoById(key);
            var product = offer.Products.FirstOrDefault(product => product is AddResourceProduct<ResourceType>);

            return product == null ? 0 : (product as AddResourceProduct<ResourceType>).Amount;
        }

        public static int GetReward<ResourceType>(this IIapService service, string key, ResourceType resourceType)
        {
            var offer = service.GetOfferInfoById(key);

            foreach (var product in offer.Products)
            {
                if (product is AddResourceProduct<ResourceType> resourceProduct &&
                    EqualityComparer<ResourceType>.Default.Equals(resourceProduct.ResourceType, resourceType))
                {
                    return resourceProduct.Amount;
                }
            }

            return 0;
        }

        public static void SetResources<ResourcesType>(this IIapService service, string key, List<TMP_Text> resources)
        {
            var offer = service.GetOfferInfoById(key);
            var products = offer.Products
                .Select(product => product is AddResourceProduct<ResourcesType> resourceProduct ? resourceProduct : null)
                .Where(product => product != null)
                .ToList();

            resources.ForEach((resource, index) =>
            {
                resource.text = $"x{products[index].Amount}";
            });
        }

        public static void SetAmount<ResourceType>(this IapProductsSettings settings, string key, ResourceType resourceType, int amount)
        {
            var offer = settings.Products.FirstOrDefault(x => x.Key == key);

            foreach (var product in offer.Products)
            {
                if (product is AddResourceProduct<ResourceType> resourceProduct &&
                    EqualityComparer<ResourceType>.Default.Equals(resourceProduct.ResourceType, resourceType))
                {
                    resourceProduct.Amount = amount;
                    break;
                }
            }
        }
    }
}
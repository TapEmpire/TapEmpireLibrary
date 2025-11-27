using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services.Offer;
using TapEmpire.Utility;
using TMPro;

namespace TapEmpire.Services
{
    public static class IIapServiceExtensions
    {
        public static IEnumerable<AddResourceProduct<ResourceType>> GetRewards<ResourceType>(this IIapService service, string key)
        {
            var offer = service.GetOfferInfoById(key);
            return offer.Products.OfType<AddResourceProduct<ResourceType>>();
        }

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

        public static void SetResources<ResourcesType>(this IIapService service, string key, IEnumerable<TMP_Text> resources)
        {
            var products = service.GetRewards<ResourcesType>(key).ToList();

            resources.ForEach((resource, index) =>
            {
                if (index >= products.Count) return;
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

        public static void SetRarity(this IapProductsSettings settings, string key, Rarity rarity)
        {
            var offer = settings.Products.FirstOrDefault(x => x.Key == key);
            offer.Rarity = rarity;
        }
    }
}
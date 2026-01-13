using System;
using System.Collections.Generic;
using WordGame.Services;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TapEmpire.Services.Shop
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/ShopSettings", fileName = "ShopSettings")]
    public class ShopSettings : ScriptableObject
    {
        public ShopUIView ShopView;
        public SerializableDictionary<InfoType, Sprite> InfoIcons;
        [SerializeReference] public List<SectionData> Sections;
    }

    [Serializable]
    public abstract class SectionData
    {
        public string Name;
        public ShopSection SectionPrefab;
    }

    [Serializable]
    public class CommonSectionData : SectionData
    {
        public BaseShopElement ShopElement;
        public List<ProductData> Products;
    }

    [Serializable]
    public class OfferSectionData : SectionData
    {
        public List<OfferData> OfferData;
    }

    [Serializable]
    public class OfferData
    {
        public string Name;
        public BundleType BundleType;
        public List<string> Products;
        public BaseShopElement ShopElement;
    }

    [Serializable]
    public class ProductData
    {
        public string Key;
        public ProductType Type;
        public InfoType InfoType;
        public Sprite Icon;

        [ShowIf("@Type == ProductType.Free || Type == ProductType.Ads || Type == ProductType.Soft")]
        [SerializeReference]
        public IProductReward Reward;
        [ShowIf("@Type == ProductType.Soft")]
        [SerializeReference]
        public IProductReward Price;
    }

    public class IProductReward
    {
        public T As<T>() where T : IProductReward { return this as T; }
    }

    [Serializable]
    public class ProductReward<ResourceType> : IProductReward
    {
        public ResourceType Resource;
        public int Amount;

        public ProductReward(ResourceType resourceType, int amount)
        {
            Resource = resourceType;
            Amount = amount;
        }

        public ProductReward() { }
    }
}
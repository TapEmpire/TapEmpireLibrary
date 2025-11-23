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
        public OfferData Offer;
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
        public ProductReward Reward;
        [ShowIf("@Type == ProductType.Soft")]
        public ProductReward Price;
    }

    [Serializable]
    public class ProductReward
    {
        public ResourceType Resource;
        public int Amount;

        public ProductReward(ResourceType resourceType, int amount)
        {
            Resource = resourceType;
            Amount = amount;
        }
    }
}
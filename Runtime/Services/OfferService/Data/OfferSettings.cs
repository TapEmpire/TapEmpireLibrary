using System;
using System.Collections.Generic;
using TapEmpire.Services.Shop;
using UnityEngine;

namespace TapEmpire.Services.Offer
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/OfferSettings", fileName = "OfferSettings")]
    public class OfferSettings : ScriptableObject
    {
        public RaritySettings Rarity;
        public SerializableDictionary<string, List<OfferType>> Placements;
        public SerializableDictionary<OfferType, OfferData> Offers;
    }

    [Serializable]
    public class OfferData
    {
        public OfferType Type;
        public BaseShopElement ShopElement;
        public SerializableDictionary<Rarity, List<string>> Products;
        [SerializeReference] public List<ICondition> Conditions = new();
    }

    public interface ICondition
    {   
    }
}
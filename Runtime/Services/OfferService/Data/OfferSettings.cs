using System;
using System.Collections.Generic;
using TapEmpire.Patterns.Strategy;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services.Offer
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/OfferSettings", fileName = "OfferSettings")]
    public class OfferSettings : ScriptableObject
    {
        public RaritySettings Rarity;
        public Rarity ButtonOfferRarity;
        public List<int> RaritySequence;
        public SerializableDictionary<string, List<OfferType>> Placements;
        public SerializableDictionary<OfferType, OfferData> Offers;
        [SerializeReference] public List<IHandler> ConditionHandlers;
        [SerializeReference] public List<ICondition> GenericConditions;
    }

    [Serializable]
    public class OfferData
    {
        public OfferType Type;
        public BaseOfferUIView Element;
        public SerializableDictionary<Rarity, List<string>> Products;
        [SerializeReference] public List<ICondition> Conditions = new();

        public OfferRuntimeData ToRuntime(Rarity rarity) => new OfferRuntimeData(this, rarity);
    }

    public class OfferRuntimeData
    {
        public OfferType Type;
        public Rarity Rarity;
        public List<string> Products;

        public OfferRuntimeData(OfferType type, Rarity rarity, List<string> products)
        {
            Type = type;
            Rarity = rarity;
            Products = products;
        }

        public OfferRuntimeData(OfferData data, Rarity rarity)
        {
            Type = data.Type;
            (Rarity, Products) = data.Products.GetValueOrFirst(rarity);
        }
    }
}
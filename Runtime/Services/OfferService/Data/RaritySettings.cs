using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TEL.Utilities;
using UnityEngine;

namespace TapEmpire.Services.Offer
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/RaritySettings", fileName = "RaritySettings")]
    public class RaritySettings : ScriptableObject
    {
        [SerializeField] private IapProductsSettings _iapSettings;
        public SerializableDictionary<Rarity, List<string>> Purchases;
        public SerializableDictionary<Rarity, RarityVisualData> Visual;

        [Button]
        private void SetPurchasesRarity()
        {
            Purchases
                .SelectMany(pair => pair.Value.Select(value => (pair.Key, value)))
                .ForEach(pair => _iapSettings.SetRarity(pair.value, pair.Key));

            EditorTools.SetDirty(_iapSettings);
        }

        [Button]
        private void GetFromPurchasesRarity()
        {
            var dictionary = _iapSettings.Products
                .GroupBy(product => product.Rarity)
                .ToDictionary(group => group.Key, group => group.Select(element => element.Key).ToList());
            Purchases = new (dictionary);
        }
    }

    [Serializable]
    public struct RarityVisualData
    {
        public Sprite Header;
        public Sprite Border;
    }
}
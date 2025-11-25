using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services.Offer
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/RaritySettings", fileName = "RaritySettings")]
    public class RaritySettings : ScriptableObject
    {
        public SerializableDictionary<Rarity, List<string>> Purchases;
        public SerializableDictionary<Rarity, RarityVisualData> Visual;

        [Button]
        private void SetPurchasesRarity()
        {
        }

        [Button]
        private void UpdatePurchasesRarity()
        {
        }
    }

    [Serializable]
    public struct RarityVisualData
    {
        public Sprite Header;
        public Sprite Border;
    }
}
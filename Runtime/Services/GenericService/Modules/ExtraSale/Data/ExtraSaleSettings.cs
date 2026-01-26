using R3;
using UnityEngine;
using TapEmpire.Services.Shop;
using WordGame.Level;
using System;
using System.Linq;
using System.Collections.Generic;
using TapEmpire.Services.Offer;

namespace TapEmpire.Modules
{
    [CreateAssetMenu(menuName = "TapEmpire/Modules/ExtraSaleSettings", fileName = "ExtraSaleSettings")]
    public class ExtraSaleSettings : ScriptableObject
    {
        public List<string> SaleList;
        public string[] Packs;
        public FlexPacks[] FlexPacks;
        public InfoType[] Labels;
        public SerializableDictionary<string, Transform> Visuals;
        public SerializableDictionary<Rarity, string> Bundles;
        public float ScrollDelay = 3.0f;
        public float WaitDelay = 3.0f;

        public string[][] GetFlexPacks() => FlexPacks.Select(data => data.Iaps).ToArray();
        public void SetFlexPacks(string[][] flexibleIaps) =>
            FlexPacks = flexibleIaps.Select(data => new FlexPacks() { Iaps = data }).ToArray();
    }

    public enum ExtraSaleType
    {
        Packs,
        PacksFlex,
        Bundle,
        BundleFlex,
        Offer,
        OfferFlex,
    }

    [Serializable]
    public class FlexPacks
    {
        public string[] Iaps;
    }
}

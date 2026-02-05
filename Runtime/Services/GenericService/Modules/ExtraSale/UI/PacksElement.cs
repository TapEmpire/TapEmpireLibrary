using System.Collections;
using System.Collections.Generic;
using TapEmpire.Services.Shop;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Modules
{
    public class PacksElement : MonoBehaviour
    {
        [SerializeField] private List<ResourcesPack> _packs;

        public void Initialize(List<ProductData> packs, List<Sprite> labels)
        {
            packs.ForEachIndexed((product, index) => _packs[index].ShopElement.Initialize(product));
            labels.ForEach((label, index) =>
            {
                _packs[index].Label.gameObject.SetActive(label != null);
                _packs[index].Label.sprite = label;
            });
        }
    }

    [System.Serializable]
    public struct ResourcesPack
    {
        public BaseShopElement ShopElement;
        public Image Label;
    }
}

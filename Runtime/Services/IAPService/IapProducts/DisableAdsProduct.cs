using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class DisableAdsProduct : IIapProduct
    {
        [SerializeField] private string _productId = "ads_remove";
        public string ProductId => _productId;
    }
}
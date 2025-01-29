using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class DisableAdsProduct : IIapProduct
    {
        [SerializeField] private string _productId = "RemoveAds";
        public string ProductId => _productId;
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    [Serializable, JsonObject]
    public class IapOffer
    {
        [Header("In game key")]
        [SerializeField, JsonProperty("Key")] private string _key;
        
        [Header("Store IDs")]
        [SerializeField, JsonProperty("AppStoreId")] private string _appStoreId;
        [SerializeField, JsonProperty("GooglePlayId")] private string _googlePlayId;
        
        [Header("Pricing")]
        [SerializeField, JsonProperty("Price")] private float _price;

        [Header("Product Type")]
        [SerializeField, JsonProperty("ProductType")] private ProductType _productType = ProductType.NonConsumable;
    
        [Header("Rewards")]
        [SerializeReference, JsonIgnore]
        private List<IIapProduct> _includedProducts = new();
        
        [JsonIgnore]
        public float Price => _price;
        [JsonIgnore]
        public IReadOnlyList<IIapProduct> Products => _includedProducts.AsReadOnly();
        [JsonIgnore]
        public ProductType ProductType => _productType;
        [JsonIgnore]
        public string Key => _key;

        public string GetStoreID()
        {
            return Application.platform switch
            {
                RuntimePlatform.Android => _googlePlayId,
                RuntimePlatform.IPhonePlayer => _appStoreId,
                _ => _googlePlayId
            };
        }
        
        public void CopyIncludedProducts(IapOffer existingOffer)
        {
            _includedProducts = new List<IIapProduct>(existingOffer._includedProducts);
        }
    }
}
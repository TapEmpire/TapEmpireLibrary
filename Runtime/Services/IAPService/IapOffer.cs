using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TapEmpire.Services.Offer;
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
        [SerializeField, JsonProperty("AppStoreId"), JsonIgnore] private string _appStoreId;
        [SerializeField, JsonProperty("GooglePlayId"), JsonIgnore] private string _googlePlayId;

        [Header("Product Type")]
        [SerializeField, JsonProperty("ProductType"), JsonIgnore] private ProductType _productType = ProductType.Consumable;
        [field: SerializeField] public Rarity Rarity { get; set; } = Rarity.Five;
    
        [Header("Rewards")]
        [SerializeReference, JsonProperty("Products")]
        private List<IIapProduct> _includedProducts = new();
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
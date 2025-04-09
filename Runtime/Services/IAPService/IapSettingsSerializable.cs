using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace TapEmpire.Services
{
    public class IapSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private IapProductsSettings _iapProductsSettings;

        public class IapRemoteModel
        {
            public List<IapOffer> Offers = new();
            public IapRemoteModel() { }
            
            public IapRemoteModel(IapProductsSettings settings)
            {
                Offers = settings.Products;
            }
        }
        
        public string TokenName => "IapSettings";
        
        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<IapRemoteModel>();
            InsertModel(model);
        }
        
        public string SerializeJson()
        {
            var model = new IapRemoteModel(_iapProductsSettings);
            var result = JsonConvert.SerializeObject(model);
            return result;
        }
        
        [Button("Serialize to file")]
        private void SerializeToFile()
        {
            var json = SerializeJson();
            FileUtility.SaveText("Save JSON", TokenName, json);
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
        
        [Button("Test Deserialize")]
        private void DeserializeIapRemoteModel(string jsonString)
        {
            var model = JsonConvert.DeserializeObject<IapRemoteModel>(jsonString);
            InsertModel(model);
        }
        
        private void InsertModel(IapRemoteModel model)
        {
            var existingOffers = _iapProductsSettings.Products.ToDictionary(offer => offer.Key);
            foreach (var offer in model.Offers)
            {
                if (existingOffers.TryGetValue(offer.Key, out var existingOffer))
                {
                    offer.CopyIncludedProducts(existingOffer);
                }
            }
            _iapProductsSettings.Products = model.Offers;
        }
    }
}
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
            public bool HasVerification = true;
            public List<IapOffer> Offers = new();
            public IapRemoteModel() { }

            public IapRemoteModel(IapProductsSettings settings)
            {
                HasVerification = settings.HasVerification;
                Offers = settings.Products;
            }
        }

        public string TokenName => "IapSettings";

        public void DeserializeJson(JToken token)
        {
            var settings = GetJsonSettings();
            var model = token.ToObject<IapRemoteModel>(JsonSerializer.Create(settings));
            InsertModel(model);
        }

        public string SerializeJson()
        {
            var model = new IapRemoteModel(_iapProductsSettings);
            var settings = GetJsonSettings();
            var result = JsonConvert.SerializeObject(model, settings);
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
            var settings = GetJsonSettings();
            var model = JsonConvert.DeserializeObject<IapRemoteModel>(jsonString, settings);
            InsertModel(model);
        }

        private void InsertModel(IapRemoteModel model)
        {
            _iapProductsSettings.HasVerification = model.HasVerification;

            var existingOffers = _iapProductsSettings.Products.ToDictionary(offer => offer.Key);
            foreach (var offer in model.Offers)
            {
                if (existingOffers.TryGetValue(offer.Key, out var existingOffer))
                {
                    existingOffer.CopyIncludedProducts(offer);
                }
            }
            // _iapProductsSettings.Products = model.Offers;
        }

        private JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                // SerializationBinder = new AllowedTypesBinder(),
            };
        }
    }
}
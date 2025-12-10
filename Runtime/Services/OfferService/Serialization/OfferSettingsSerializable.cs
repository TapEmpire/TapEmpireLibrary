using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services.Offer
{
    [Serializable]
    public class OfferSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] protected OfferSettings _settings;

        public class OfferSettingsRemoteModel
        {
            public Rarity ButtonOfferRarity = Rarity.Fifty;
            public List<int> RaritySequence = new () { 0 };

            public OfferSettingsRemoteModel() { }

            public OfferSettingsRemoteModel(OfferSettings settings)
            {
                ButtonOfferRarity = settings.ButtonOfferRarity;
                RaritySequence = settings.RaritySequence;
            }

            public virtual void ToSettings(OfferSettings settings)
            {
                settings.ButtonOfferRarity = ButtonOfferRarity;
                settings.RaritySequence = RaritySequence;
            }
        }

        public string TokenName => "OfferSettings";

        public virtual void DeserializeJson(JToken token)
        {
            var model = token.ToObject<OfferSettingsRemoteModel>();
            model.ToSettings(_settings);
        }

        public virtual string SerializeJson()
        {
            var model = new OfferSettingsRemoteModel(_settings);
            var result = JsonConvert.SerializeObject(model);

            return result;
        }

        [Button("Serialize to console")]
        private void SerializeToConsole()
        {
            var json = SerializeJson();
            Debug.Log(json);
        }
    }
}
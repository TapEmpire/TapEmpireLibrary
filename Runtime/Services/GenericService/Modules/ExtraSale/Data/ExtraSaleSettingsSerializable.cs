using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Services;
using TapEmpire.Services.Shop;
using UnityEngine;

namespace TapEmpire.Modules
{
    [Serializable]
    public class ExtraSaleSettingsSerializable : IRemoteSerializable
    {
        [SerializeField] private ExtraSaleSettings _settings;

        public class ExtraSaleSettingsRemoteModel
        {
            public string[] SaleList = null;
            public string[] Packs = null;
            public string[][] FlexPacks = null;
            public InfoType[] Labels = null;
            public float ScrollDelay = 3.0f;
            public float WaitDelay = 3.0f;

            public ExtraSaleSettingsRemoteModel() { }

            public ExtraSaleSettingsRemoteModel(ExtraSaleSettings settings)
            {
                SaleList = settings.SaleList;
                Packs = settings.Packs;
                FlexPacks = settings.GetFlexPacks();
                Labels = settings.Labels;
                ScrollDelay = settings.ScrollDelay;
                WaitDelay = settings.WaitDelay;
            }

            public void ToSettings(ExtraSaleSettings settings)
            {
                settings.ScrollDelay = ScrollDelay;
                settings.WaitDelay = WaitDelay;
                settings.SetFlexPacks(FlexPacks ?? Array.Empty<string[]>());

                if (SaleList != null)
                {
                    settings.SaleList = SaleList.ToArray();
                }

                if (Packs != null)
                {
                    settings.Packs = Packs.ToArray();
                }

                if (Labels != null)
                {
                    settings.Labels = Labels.ToArray();
                }
            }
        }

        public string TokenName => "ExtraSaleSettings";

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<ExtraSaleSettingsRemoteModel>();
            model.ToSettings(_settings);
        }

        public string SerializeJson()
        {
            var model = new ExtraSaleSettingsRemoteModel(_settings);
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
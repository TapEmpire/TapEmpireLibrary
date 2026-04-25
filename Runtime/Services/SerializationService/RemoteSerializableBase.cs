using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IRemoteModel<TSettings>
    {
        void FromSettings(TSettings settings);
        void ToSettings(TSettings settings);
    }

    [Serializable]
    public abstract class RemoteSerializableBase<TSettings, TModel> : IRemoteSerializable
        where TModel : IRemoteModel<TSettings>, new()
    {
        [SerializeField] protected TSettings _settings;

        public abstract string TokenName { get; }

        public void DeserializeJson(JToken token)
        {
            var model = token.ToObject<TModel>();
            model.ToSettings(_settings);
        }

        public string SerializeJson()
        {
            var model = new TModel();
            model.FromSettings(_settings);
            return JsonConvert.SerializeObject(model);
        }

        [Button("Serialize to console")]
        private void SerializeToConsole() => Debug.Log(SerializeJson());
    }
}

using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TapEmpire.Services
{
    [Serializable, JsonObject]
    public class IapSettings
    {
        [JsonProperty]
        [field: SerializeField] public string GameId { get; private set; } = "";
        [JsonProperty]
        [field: SerializeField] public string Key { get; set; } = "";
        [JsonProperty]
        [field: SerializeField] public float Price { get; private set; } = 0;
        [field: NonSerialized] public virtual ProductType ProductType { get; private set; }
        [JsonProperty]
        [field: SerializeField] public bool Enabled { get; private set; } = true;
    }
}
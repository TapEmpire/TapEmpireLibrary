using System;
using Newtonsoft.Json;

namespace TapEmpire.Services
{
    [Serializable]
    public class PatchEntryBase
    {
        [JsonProperty("version")] public int Version;
        [JsonProperty("deviceId")] public string DeviceId;
        [JsonProperty("uuid")] public string Uuid;
        [JsonProperty("deviceIdHash")] public string DeviceIdHash;
    }
}

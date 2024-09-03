using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public class QuaternionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Quaternion quaternion = (Quaternion)value;
            serializer.Serialize(writer, new { x = quaternion.x, y = quaternion.y, z = quaternion.z, w = quaternion.w });
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Quaternion((float)jo["x"], (float)jo["y"], (float)jo["z"], (float)jo["w"]);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(Quaternion);
    }
}
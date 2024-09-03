using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public class Vector3Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3 vector = (Vector3)value;
            serializer.Serialize(writer, new { x = vector.x, y = vector.y, z = vector.z });
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Vector3((float)jo["x"], (float)jo["y"], (float)jo["z"]);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(Vector3);
    }
}
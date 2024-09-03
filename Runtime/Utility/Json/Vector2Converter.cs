using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace TapEmpire.Utility
{
    public class Vector2Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector2 vector = (Vector2)value;
            serializer.Serialize(writer, new { x = vector.x, y = vector.y });
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Vector2((float)jo["x"], (float)jo["y"]);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2);
    }
}

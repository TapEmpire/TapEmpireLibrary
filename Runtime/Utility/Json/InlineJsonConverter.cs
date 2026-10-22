using System;
using Newtonsoft.Json;

namespace TapEmpire.Utility
{
    // Writes a value on a single line while the document around it stays indented. Point a
    // JsonProperty's ItemConverterType at it to keep the elements of a long list one per line.
    // WriteRawValue emits the enclosing indent first, so the elements still break as usual.
    public class InlineJsonConverter : JsonConverter
    {
        private static readonly JsonSerializerSettings Inner = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(JsonConvert.SerializeObject(value, Inner));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
            => throw new NotSupportedException();
    }
}

using Newtonsoft.Json.Linq;

namespace TapEmpire.Utility
{
    public static class JsonExtensions
    {
        public static JObject FlattenArrayToObject(this JArray array)
        {
            JObject result = new JObject();

            foreach (JToken item in array)
            {
                if (item is JObject obj)
                {
                    obj.Properties().ForEach(property => result[property.Name] = property.Value);
                }
            }

            return result;
        }
    }
}
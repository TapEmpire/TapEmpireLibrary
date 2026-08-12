using Newtonsoft.Json;

namespace TapEmpire.Utility
{
    public static class UnityJsonSettings
    {
        public static readonly JsonSerializerSettings Default = new()
        {
            Converters =
            {
                new Vector2Converter(),
                new Vector3Converter(),
                new QuaternionConverter(),
            },
        };
    }
}

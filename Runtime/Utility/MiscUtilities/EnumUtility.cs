
namespace TapEmpire.Utility
{
    public static class EnumUtility
    {
        public static TValue Parse<TValue>(string value) where TValue : System.Enum
        {
            return (TValue)System.Enum.Parse(typeof(TValue), value);
        }

        public static TValue? TryParse<TValue>(string value) where TValue : struct
        {
            TValue result;
            return System.Enum.TryParse<TValue>(value, out result) ? result : null;
        }

        public static TValue GetRandomValue<TValue>() where TValue : System.Enum
        {
            return GetRandomValueInternal<TValue>(0);
        }

        public static TValue GetRandomValueFromSecond<TValue>() where TValue : System.Enum
        {
            return GetRandomValueInternal<TValue>(1);
        }

        private static TValue GetRandomValueInternal<TValue>(int start) where TValue : System.Enum
        {
            var values = System.Enum.GetNames(typeof(TValue));
            var random = new System.Random();
            var value = values[random.Next(start, values.Length)];

            return Parse<TValue>(value);
        }
    }
}
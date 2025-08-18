
using System.Collections.Generic;
using System.Linq;

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

        public static TValue Parse<TValue>(int value) where TValue : System.Enum
        {
            return (TValue)System.Enum.ToObject(typeof(TValue), value);
        }

        public static TValue GetRandomValue<TValue>() where TValue : System.Enum
        {
            return GetRandomValueInternal<TValue>(0);
        }

        public static TValue GetRandomValueFromSecond<TValue>() where TValue : System.Enum
        {
            return GetRandomValueInternal<TValue>(1);
        }

        public static TValue GetRandomValue<TValue>(int start, int end) where TValue : System.Enum
        {
            return GetRandomValueInternal<TValue>(start, end);
        }

        private static TValue GetRandomValueInternal<TValue>(int start, int end = 0) where TValue : System.Enum
        {
            var values = System.Enum.GetNames(typeof(TValue));
            var random = new System.Random();
            end = end > 0 ? end : values.Length;
            var value = values[random.Next(start, end)];

            return Parse<TValue>(value);
        }

        public static Dictionary<T, U> CreateDefaultDictionary<T, U>(U defaultValue) where T : System.Enum
        {
            return System.Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(key => key, _ => defaultValue);
        }

        public static IEnumerable<T> CreateIEnumerable<T>() where T : System.Enum
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
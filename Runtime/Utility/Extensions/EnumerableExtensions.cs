using System.Collections.Generic;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class EnumerableHelper<E>
    {
        private static System.Random random;

        static EnumerableHelper()
        {
            random = new System.Random();
        }

        public static T Random<T>(IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(random.Next(enumerable.Count()));
        }
    }

    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            return EnumerableHelper<T>.Random(enumerable);
        }

        public static bool Empty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 0;
        }

        public static bool NonEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() > 0;
        }

        public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
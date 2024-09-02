using System;
using System.Collections.Generic;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class EnumerableUtility
    {
        public static (IEnumerable<TSource>, IEnumerable<TSource>) Partition<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var firstPart = source.Where(predicate);
            var secondPart = source.Where((source) => !predicate(source));

            return (firstPart, secondPart);
        }

        public static void ForEachIndexed<TSource>(this IEnumerable<TSource> source, System.Action<TSource, int> action)
        {
            int index = 0;

            foreach (var element in source)
            {
                action?.Invoke(element, index++);
            }
        }
    }
}
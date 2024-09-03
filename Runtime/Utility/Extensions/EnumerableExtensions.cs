using System;
using System.Collections.Generic;
using System.Linq;

namespace TapEmpire.Utility
{
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

        public static bool TryGetIndex<T>(this T[] self, T indexOfItem, out int index) where T : class
        {
            for (var i = 0; i < self.Length; i++)
            {
                var item = self[i];
                if (item == indexOfItem)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        
        public static bool TryGetFirst<T>(this IEnumerable<T> self, Predicate<T> predicate, out T value)
        {
            foreach (var item in self)
            {
                if (!predicate(item))
                {
                    continue;
                }

                value = item;
                return true;
            }

            value = default;
            return false;
        }
        
        public static T GetWithMax<T>(this IEnumerable<T> self, Func<T, float> getValueDelegate)
        {
            var max = float.NegativeInfinity;
            T maxItem = default;
            foreach (var item in self)
            {
                var value = getValueDelegate.Invoke(item);
                if (value > max)
                {
                    max = value;
                    maxItem = item;
                }
            }
            return maxItem;
        }
        
        public static T GetWithMin<T>(this IEnumerable<T> self, Func<T, float> getValueDelegate)
        {
            var min = float.PositiveInfinity;
            T minItem = default;
            foreach (var item in self)
            {
                var value = getValueDelegate.Invoke(item);
                if (value < min)
                {
                    min = value;
                    minItem = item;
                }
            }
            return minItem;
        }
        
        public static bool TryGetAt<T>(this IList<T> self, int index, out T item)
        {
            if (self.Count > index)
            {
                item = self[index];
                return true;
            }
            item = default;
            return false;
        }
        
        public static T[] RemoveItem<T>(this T[] self, T itemToRemove)
        {
            // Find the index of the item to remove.
            int index = Array.IndexOf(self, itemToRemove);
        
            // If the item is not found, return the original array.
            if (index < 0)
            {
                return self;
            }
        
            // Create a new array with a size one less than the original.
            T[] newArray = new T[self.Length - 1];
        
            // Copy the items before the removed item.
            if (index > 0)
            {
                Array.Copy(self, 0, newArray, 0, index);
            }
        
            // Copy the items after the removed item.
            if (index < self.Length - 1)
            {
                Array.Copy(self, index + 1, newArray, index, self.Length - index - 1);
            }
        
            return newArray;
        }
    }

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
}
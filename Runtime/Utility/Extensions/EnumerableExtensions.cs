using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace TapEmpire.Utility
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            return EnumerableHelper<T>.Random(enumerable);
        }
        
        public static T Random<T>(this IEnumerable<T> enumerable, Random random)
        {
            return EnumerableHelper<T>.Random(enumerable, random);
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

        public static void ForEach<TSource>(this IEnumerable<TSource> source, System.Action<TSource, int> action)
        {
            int index = 0;

            foreach (var element in source)
            {
                action?.Invoke(element, index++);
            }
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, int count)
        {
            return source.Where(predicate).Take(count);
        }

        public static (IEnumerable<TSource>, IEnumerable<TSource>) Partition<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var firstPart = source.Where(predicate);
            var secondPart = source.Where((source) => !predicate(source));

            return (firstPart, secondPart);
        }

        public static IEnumerable<TSource> SortByOrder<TSource, TValue>(this IEnumerable<TSource> source, IEnumerable<TValue> order,
            Func<TSource, TValue> converter)
        {
            var dictionary = order
                .Select((value, index) => (value, index))
                .ToDictionary(pair => pair.value, pair => pair.index);

            var offset = dictionary.Count;

            source.ForEach((value, index) => dictionary.TryAdd(converter.Invoke(value), index + offset));

            return source.OrderBy(value => dictionary[converter(value)]);
        }

        public static void ForEachIndexed<TSource>(this IEnumerable<TSource> source, System.Action<TSource, int> action)
        {
            int index = 0;

            foreach (var element in source)
            {
                action?.Invoke(element, index++);
            }
        }

        public static void ForEachIndexed<TSource>(this IEnumerable<TSource> source, System.Action<int> action)
        {
            for (int i = 0; i < source.Count(); ++i)
            {
                action?.Invoke(i);
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static LinkedListNode<T> First<T>(this LinkedList<T> list, Func<T, bool> condition, int from = 0)
        {
            var count = 0;
            foreach (var node in list)
            {
                if (count < from)
                {
                    count++;
                    continue;
                }
                
                if (condition(node))
                {
                    return list.Find(node);
                }
            }

            return null;
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

        public static bool TryGetFirstOfType<T1, T2>(this IEnumerable<T1> self, out T2 value)
        {
            foreach (var item in self)
            {
                if (item is not T2 itemOfType)
                {
                    continue;
                }
                value = itemOfType;
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

        public static Dictionary<TKey, int> CountByExt<T, TKey>(this IEnumerable<T> self, Func<T, TKey> getKeyDelegate)
        {
            var counts = new Dictionary<TKey, int>();
            foreach (var item in self)
            {
                var key = getKeyDelegate.Invoke(item);
                counts[key] = counts.TryGetValue(key, out var count) ? count + 1 : 1;
            }
            return counts;
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

        public static (T First, T Second) FindFirstPairByCallback<T, TResult>(this IEnumerable<T> source, Func<T, TResult> callback)
        {
            var dictionary = new Dictionary<TResult, T>();

            foreach (var item in source)
            {
                var result = callback(item);

                if (dictionary.TryGetValue(result, out var existingItem))
                {
                    return (existingItem, item);
                }

                dictionary[result] = item;
            }

            return default;
        }

        public static (T First, T Second) FindPairByCallback<T, TResult>(this IEnumerable<T> source, Func<T, TResult> callback, Func<T, int> extraCondition)
        {
            var dictionary = new Dictionary<TResult, List<T>>();

            foreach (var item in source)
            {
                var result = callback(item);

                if (dictionary.TryGetValue(result, out var existingList))
                {
                    var extraValue = extraCondition(item);
                    var existingItem = existingList.Find(element => extraCondition(element) != extraValue);
                    if (existingItem != null)
                    {
                        return (existingItem, item);
                    }

                    existingList.Add(item);
                    continue;
                }

                dictionary[result] = new List<T>() { item };
            }

            var matches = dictionary.FirstOrDefault(pair => pair.Value.Count >= 2);

            if (matches.Value != null)
            {
                return (matches.Value[0], matches.Value[1]);
            }

            return default;
        }
        
        public static List<T> FindItemsByCallback<T, TResult>(this IEnumerable<T> source, Func<T, TResult> callback, int count)
        {
            var dictionary = new Dictionary<TResult, List<T>>();

            foreach (var item in source)
            {
                var result = callback(item);

                if (dictionary.TryGetValue(result, out var existingList))
                {
                    existingList.Add(item);
                    continue;
                }

                dictionary[result] = new List<T>() {item};
            }

            var matches = dictionary.FirstOrDefault(pair => pair.Value.Count >= count);

            if (matches.Value != null)
            {
                return matches.Value.ToList();
            }

            return default;
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
        
        public static T Random<T>(IEnumerable<T> enumerable, Random otherRandom)
        {
            return enumerable.ElementAt(otherRandom.Next(enumerable.Count()));
        }
    }
}
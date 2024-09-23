using System;
using System.Collections.Generic;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class ListExtensions
    {
        public static bool Empty<T>(this List<T> list)
        {
            return list.Count == 0;
        }

        public static T Pop<T>(this List<T> list)
        {
            int lastIndex = list.Count - 1;
            T lastElement = list[lastIndex];
            list.RemoveAt(lastIndex);
            return lastElement;
        }

        public static T PopSafe<T>(this List<T> list)
        {
            return list.Empty() ? default(T) : list.Pop();
        }

        public static T PopFront<T>(this List<T> list)
        {
            T firstElement = list.First();
            list.RemoveAt(0);
            return firstElement;
        }

        public static T PopFrontSafe<T>(this List<T> list)
        {
            return list.Empty() ? default(T) : list.PopFront();
        }

        public static void RemoveFrom<T>(this List<T> list, int from)
        {
            list.RemoveRange(from, list.Count - from);
        }

        public static void RemoveLast<T>(this List<T> list, int count)
        {
            list.RemoveRange(list.Count - count, count);
        }

        public static (T, int) FindLastAndRemove<T>(this List<T> list, Func<T, bool> condition) where T : new()
        {
            var index = list.FindLastIndex(x => condition(x));
            if (index == -1)
            {
                return (default(T), index);
            }

            var element = list[index];
            list.RemoveAt(index);
            return (element, index);
        }

        public static void Resize<T>(this List<T> list, int count, T value = default(T))
        {
            int listCount = list.Count;
            if (count < listCount)
            {
                list.RemoveRange(count, listCount - count);
            }
            else if (count > listCount)
            {
                list.AddRange(Enumerable.Repeat(value, count - listCount));
            }
        }

        public static void ClearResize<T>(this List<T> list, int count, T value = default(T))
        {
            list.Clear();
            list.AddRange(Enumerable.Repeat(value, count));
        }

        public static void ResizeAndFill<T>(this List<T> list, int count, Func<int, T> createMethod)
        {
            list.ClearResize(count);
            for (int i = 0; i < count; ++i)
            {
                list[i] = createMethod(i);
            }
        }

        public static void ResizeAndFill<T>(this List<T> list, int count, Action<int> createMethod)
        {
            list.ClearResize(count);
            for (int i = 0; i < count; ++i)
            {
                createMethod(i);
            }
        }

        public static void AddAndFill<T>(this List<T> list, int count, Action<int> createMethod)
        {
            var oldCount = list.Count;
            list.Resize(count);
            for (int i = oldCount; i < list.Count; ++i)
            {
                createMethod(i);
            }
        }

        public static void Swap<T>(this List<T> list, int first, int second)
        {
            if (first == second)
            {
                return;
            }

            T item = list[first];
            list[first] = list[second];
            list[second] = item;
        }

        public static void ForEachIndexed<T>(this List<T> list, System.Action<T, int> action)
        {
            var count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                action?.Invoke(list[i], i);
            }
        }

        public static T GetRandomElement<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count - 1)];
        }
    }
}
using System;

namespace TapEmpire.Utility
{
    public static class ArrayUtility
    {
        public static void AddOrCreate<T>(ref T[] array, T item)
        {
            if (array == null || array.Length == 0)
            {
                array = new [] { item };
                return;
            }
            var newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, array.Length);
            newArray[^1] = item;
        }
    }
}
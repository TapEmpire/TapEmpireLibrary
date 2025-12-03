using System;
using System.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class EnumExtensions
    {
        public static int ToInt<TValue>(this TValue value) where TValue : Enum
            => (int)(object)value;

        public static bool HasAnyFlags<TValue>(this TValue value, params TValue[] flag) where TValue : System.Enum
        {
            return flag.Any(flag => value.HasFlag(flag));
        }

        public static TValue Next<TValue>(this TValue value) where TValue : Enum
        {
            return value.NextSafe();
        }

        public static TValue NextSafe<TValue>(this TValue value) where TValue : Enum
        {
            TValue[] values = (TValue[])Enum.GetValues(typeof(TValue));
            int currentIndex = Array.IndexOf(values, value);
            int nextIndex = MathUtility.LoopClamp(currentIndex + 1, values.Length);
            return values[nextIndex];
        }

        public static TValue Add<TValue>(this TValue value, int delta) where TValue : Enum
        {
            TValue[] values = (TValue[])Enum.GetValues(typeof(TValue));
            int currentIndex = Array.IndexOf(values, value);
            int sum = Mathf.Clamp(currentIndex + delta, 0, values.Length - 1);
            return values[sum];
        }
    }
}
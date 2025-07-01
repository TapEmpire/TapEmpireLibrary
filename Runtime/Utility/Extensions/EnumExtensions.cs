using System;
using System.Linq;

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
    }
}
using System;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class EnumExtensions
    {
        public static int EnumToInt<TValue>(this TValue value) where TValue : Enum
            => (int)(object)value;

        public static bool HasAnyFlags<TValue>(this TValue value, params TValue[] flag) where TValue : System.Enum
        {
            return flag.Any(flag => value.HasFlag(flag));
        }
    }
}
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace TapEmpire.Utility
{
    public static class PoolUtility<T>
        where T : class, new()
    {
        private static readonly Stack<T> VALUES = new();

        public static void Push(T value)
        {
            Assert.IsFalse(VALUES.Contains(value));
            VALUES.Push(value);
        }

        public static T Pull()
        {
            return VALUES.Count > 0
                ? VALUES.Pop()
                : new();
        }
    }
}
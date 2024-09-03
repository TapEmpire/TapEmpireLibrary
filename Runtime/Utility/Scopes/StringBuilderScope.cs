using System;
using System.Text;

namespace TapEmpire.Utility
{
    public readonly struct StringBuilderScope : IDisposable
    {
        private readonly StringBuilder _stringBuilder;

        private StringBuilderScope(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
        }

        public static StringBuilderScope Create(out StringBuilder stringBuilder)
        {
            stringBuilder = PoolUtility<StringBuilder>.Pull();
            return new StringBuilderScope(stringBuilder);
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            _stringBuilder.Clear();
            PoolUtility<StringBuilder>.Push(_stringBuilder);
        }

        #endregion
    }
}
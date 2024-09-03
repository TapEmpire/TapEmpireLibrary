using System;
using System.Diagnostics;

namespace TapEmpire.Utility
{
    public readonly struct StopwatchScope : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        private StopwatchScope(Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
        }

        public static StopwatchScope Create(out Stopwatch stopwatch)
        {
            stopwatch = PoolUtility<Stopwatch>.Pull();
            return new(stopwatch);
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            _stopwatch.Reset();
            PoolUtility<Stopwatch>.Push(_stopwatch);
        }

        #endregion
    }
}
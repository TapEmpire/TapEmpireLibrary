using System;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Utility
{
    public sealed class SharedDisposable<T> : IDisposable
    {
        public T Value { get; }

        public UniTask Released => _released.Task;

        private readonly Action<T> _release;
        private readonly UniTaskCompletionSource _released = new();

        private int _holdCount = 1;
        private bool _isReleased;

        public SharedDisposable(T value, Action<T> release)
        {
            Value = value;
            _release = release;
        }

        public IDisposable Hold()
        {
            if (_isReleased) return Disposable.Empty;

            _holdCount++;

            return Disposable.Create(ReleaseHold);
        }

        public void Dispose() => ReleaseHold();

        private void ReleaseHold()
        {
            if (_isReleased || --_holdCount > 0) return;

            _isReleased = true;
            _release(Value);
            _released.TrySetResult();
        }
    }
}

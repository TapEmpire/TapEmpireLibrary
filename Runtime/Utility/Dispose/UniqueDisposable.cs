using System;

namespace TapEmpire.Utility
{
    public sealed class UniqueDisposable : IDisposable
    {
        private IDisposable _current;
        private bool _isDisposed;

        public IDisposable Disposable
        {
            get => _current;
            set
            {
                if (_isDisposed)
                {
                    value?.Dispose();
                    return;
                }

                _current?.Dispose();
                _current = value;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _current?.Dispose();
            _current = null;
        }
    }
}

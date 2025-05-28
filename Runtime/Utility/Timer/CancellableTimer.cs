using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using R3;

namespace TapEmpire.Utility
{
    public class CancellableTimer : IDisposable
    {
        public Subject<Unit> OnDone = new();

        private float _delay = 0.0f;
        private CancellationTokenSource _tokenSource;

        public CancellableTimer(float delay, Action callback)
        {
            _delay = delay;

            if (callback != null)
            {
                OnDone.Subscribe(_ => callback.Invoke());
            }
        }

        public void Restart()
        {
            Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new();

            Run().Forget();
        }

        public void Restart(float delay)
        {
            _delay = delay;
            Restart();
        }

        public void Cancel()
        {
            _tokenSource?.Cancel();
        }

        public void Dispose()
        {
            Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            OnDone.Dispose();
        }

        private async UniTask Run()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_delay), cancellationToken: _tokenSource.Token);
                OnDone.OnNext(Unit.Default);
            }
            finally
            {
            }
        }
    }
}
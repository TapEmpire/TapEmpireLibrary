using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;
using R3;

namespace TapEmpire.Utility
{
    public class UniTaskInterval : IDisposable
    {
        public Subject<int> OnInterval = new();

        private bool _isRunning = true;
        private int _counter = 0;
        private CancellationTokenSource _cancellationTokenSource = new();

        public async UniTask Run(float tickInterval)
        {
            try
            {
                while (_isRunning)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(tickInterval), cancellationToken: _cancellationTokenSource.Token);
                    OnInterval.OnNext(++_counter);
                }
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public static UniTaskInterval Create(float tickInterval)
        {
            var timer = new UniTaskInterval();
            timer.Run(tickInterval).Forget();
            return timer;
        }
    }
}
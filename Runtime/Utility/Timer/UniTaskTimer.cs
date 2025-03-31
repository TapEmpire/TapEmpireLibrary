using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;
using R3;

namespace TapEmpire.Utility
{
    public class UniTaskTimer : IDisposable
    {
        public ReadOnlyReactiveProperty<float> OnTimeLeft => _timeLeft;
        public Subject<bool> OnTimerDone = new();

        private ReactiveProperty<float> _timeLeft = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        private async UniTask Run(float duration, float tickInterval)
        {
            try
            {
                float elapsedTime = 0.0f;

                while (elapsedTime < duration)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(tickInterval), cancellationToken: _cancellationTokenSource.Token);

                    elapsedTime += tickInterval;
                    _timeLeft.Value = Mathf.Max(duration - elapsedTime, 0.0f);
                }

                OnTimerDone.OnNext(true);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _timeLeft.Dispose();
        }

        public static UniTaskTimer Create(float duration, float tickInterval)
        {
            var timer = new UniTaskTimer();
            timer.Run(duration, tickInterval).Forget();
            return timer;
        }
    }
}
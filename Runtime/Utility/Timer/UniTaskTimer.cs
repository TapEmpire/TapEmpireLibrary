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
        private CancellableTask _cancellableTask = null;

        private async UniTask Run(float duration, float tickInterval, float startValue = 0.0f)
        {
            try
            {
                float elapsedTime = startValue;
                _timeLeft.Value = duration - elapsedTime;

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

        // Awaits only the delay, not the timer itself.
        private async UniTask RunWithDelay(float duration, float tickInterval, float delay)
        {
            _cancellableTask = UniTaskUtility.Delay(delay, () => Run(duration, tickInterval).Forget());
            await _cancellableTask.AsUniTask();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellableTask?.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _timeLeft.Dispose();
            _cancellableTask?.Dispose();
            OnTimerDone.Dispose();
        }

        public static UniTaskTimer Create(float duration, float tickInterval, float startValue = 0.0f)
        {
            var timer = new UniTaskTimer();
            timer.Run(duration, tickInterval, startValue).Forget();
            return timer;
        }

        public static UniTaskTimer CreateWithDelay(float duration, float tickInterval, float delay)
        {
            var timer = new UniTaskTimer();
            timer.RunWithDelay(duration, tickInterval, delay).Forget();
            return timer;
        }
    }
}
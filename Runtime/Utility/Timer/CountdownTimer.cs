using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public class CountdownTimer : IDisposable
    {
        public Action<string> OnTick;
        public Action OnExpired;

        private readonly Func<double> _getRemainingSeconds;
        private readonly TimerDisplayFormat _format;
        private readonly float _tickInterval;
        private CancellationTokenSource _cts;

        public CountdownTimer(Func<double> getRemainingSeconds, TimerDisplayFormat format, float tickInterval = 1f)
        {
            _getRemainingSeconds = getRemainingSeconds;
            _format = format;
            _tickInterval = tickInterval;
        }

        public void Start()
        {
            Stop();
            _cts = new CancellationTokenSource();
            TickLoop(_cts.Token).Forget();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            Stop();
            OnTick = null;
            OnExpired = null;
        }

        private async UniTaskVoid TickLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var remaining = Math.Max(0, _getRemainingSeconds());
                var span = TimeSpan.FromSeconds(remaining);

                var text = _format switch
                {
                    TimerDisplayFormat.DayHour => $"{(int)span.TotalDays:D2}:{span.Hours:D2}",
                    TimerDisplayFormat.HourMinute => $"{(int)span.TotalHours:D2}:{span.Minutes:D2}",
                    TimerDisplayFormat.MinuteSecond => $"{(int)span.TotalMinutes:D2}:{span.Seconds:D2}",
                    _ => $"{(int)span.TotalMinutes:D2}:{span.Seconds:D2}"
                };

                OnTick?.Invoke(text);

                if (remaining <= 0)
                {
                    OnExpired?.Invoke();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_tickInterval), cancellationToken: ct);
            }
        }
    }
}

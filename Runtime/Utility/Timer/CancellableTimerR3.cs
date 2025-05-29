using System;
using R3;
using UnityEngine;

namespace TapEmpire.Utility
{
    // TODO: The callback is not invoked on the unity thread!
    public class CancellableTimerR3 : IDisposable
    {
        public Subject<Unit> OnDone = new();

        private float _delay = 0.0f;
        private IDisposable _subscription;

        public CancellableTimerR3(float delay, Action callback = null)
        {
            _delay = delay;

            if (callback != null)
            {
                OnDone.Subscribe(_ => callback.Invoke());
            }
        }

        public void Restart()
        {
            _subscription = Observable
                .Timer(TimeSpan.FromSeconds(_delay))
                // .ObserveOn(UnityFrameProvider.)
                .Subscribe(OnDone.AsObserver());
        }

        public void Cancel()
        {
            _subscription?.Dispose();
        }

        public void Dispose()
        {
            Cancel();
            OnDone.Dispose();
        }
    }
}
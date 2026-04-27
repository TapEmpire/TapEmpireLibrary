using System;
using R3;
using TapEmpire.Services;
using TMPro;
using UnityEngine;

namespace TapEmpire.Utility
{
    public class CountdownTimerView : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text _text;

        public Action OnExpired;

        private Func<TimeSpan> _getRemainingTime;
        private IDisposable _subscription;

        public IDisposable Initialize(Func<TimeSpan> getRemainingTime, ISystemService systemService)
        {
            _getRemainingTime = getRemainingTime;
            UpdateTimer();
            _subscription = systemService.OnTick.Subscribe(_ => UpdateTimer());
            return this;
        }

        private void UpdateTimer()
        {
            var remaining = _getRemainingTime();
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;
            _text.text = CountdownFormatUtility.Format(remaining);
            if (remaining <= TimeSpan.Zero)
                OnExpired?.Invoke();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}

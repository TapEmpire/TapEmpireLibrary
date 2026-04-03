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

        private Func<double> _getRemainingSeconds;
        private IDisposable _subscription;

        public IDisposable Initialize(Func<double> getRemainingSeconds, ISystemService systemService)
        {
            _getRemainingSeconds = getRemainingSeconds;
            UpdateTimer();
            _subscription = systemService.OnTick.Subscribe(_ => UpdateTimer());
            return this;
        }

        private void UpdateTimer()
        {
            var remaining = Math.Max(0, _getRemainingSeconds());
            _text.text = CountdownFormatUtility.Format(remaining);
            if (remaining <= 0)
                OnExpired?.Invoke();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    public class SessionService : Initializable, ISessionService
    {
        private const string TotalTimeKey = "TotalSessionTime";

        private DateTime _inactiveStartTime;
        private bool _isTracking;

        [Inject]
        private DiContainer _diContainer;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _diContainer.Inject(this);
            Application.focusChanged += OnFocusChanged;
            Application.quitting += SaveInactiveTime;

            if (!Application.isFocused)
            {
                StartTracking();
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        public TimeSpan GetTotalInactiveTime()
        {
            return TimeSpan.FromSeconds(PlayerPrefs.GetFloat(TotalTimeKey, 0));
        }

        public void ResetTotalInactiveTime()
        {
            PlayerPrefs.DeleteKey(TotalTimeKey);
            PlayerPrefs.Save();
        }

        private void StartTracking()
        {
            _inactiveStartTime = DateTime.UtcNow;
            _isTracking = true;
        }

        private void StopTracking()
        {
            if (!_isTracking) return;

            var inactiveDuration = DateTime.UtcNow - _inactiveStartTime;
            var totalInactiveTime = PlayerPrefs.GetFloat(TotalTimeKey, 0);
            totalInactiveTime += (float)inactiveDuration.TotalSeconds;
            PlayerPrefs.SetFloat(TotalTimeKey, totalInactiveTime);
            PlayerPrefs.Save();
            _isTracking = false;
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                StopTracking();
            }
            else
            {
                StartTracking();
            }
        }

        private void SaveInactiveTime()
        {
            StopTracking();
        }

        protected override void OnRelease()
        {
            Application.focusChanged -= OnFocusChanged;
            Application.quitting -= SaveInactiveTime;
        }
    }
}
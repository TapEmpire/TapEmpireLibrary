using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using TapEmpire.Utility;
using Debug = UnityEngine.Debug;

namespace TapEmpire.Services
{
    [Serializable]
    public class HapticService : Initializable, IHapticService
    {
        [SerializeField]
        private bool _isDefaultVibration = true;

        [SerializeField]
        [HideIf(nameof(_isDefaultVibration))]
        private HapticTypes _defaultHapticType = HapticTypes.None;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            SetHapticsActive(PlayerPrefsUtility.GetHapticSettings(true), false);
            return UniTask.CompletedTask;
        }
        
        protected override void OnRelease()
        {
        }
        
        public bool IsHapticsActive { get; private set; }
        
        public void Vibrate()
        {
            MMVibrationManager.Vibrate();
        }

        public void PlayDefaultVibration()
        {
            //Logger.Log("PlayDefaultVibration");
            this.PlayVibration(_isDefaultVibration, _defaultHapticType);
        }

        public void SetHapticsActive(bool active, bool playHapticOnOff)
        {
            IsHapticsActive = active;
            PlayerPrefsUtility.SetHapticSettings(active);

            MMVibrationManager.SetHapticsActive(active);
            if (playHapticOnOff)
            {
                MMVibrationManager.Haptic(active ? HapticTypes.Success : HapticTypes.Failure, false, true);
            }
        }

        public void PlayHaptic(HapticTypes type)
        {
            if (!IsHapticsActive)
            {
                return;
            }
            Log(type);
            MMVibrationManager.Haptic(type);
        }

        [Conditional("UNITY_EDITOR")]
        private void Log(HapticTypes type)
        {
            Debug.Log("[HAPTIC] Play haptic: " + type);
        }
    }
}

using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    public class HapticSource : MonoBehaviour
    {
        [SerializeField]
        private bool _isDefaultVibration = true;

        [SerializeField]
        [HideIf(nameof(_isDefaultVibration))]
        private HapticTypes _specificHapticType = HapticTypes.None;

        [SerializeField]
        private bool _limitRate = true;

        [SerializeField]
        [ShowIf(nameof(_limitRate))]
        private float _limitRateInterval = 0.5f;

        private float _lastPlayTime;
        
        private IHapticService _hapticService;

        [Inject]
        private void Construct(IHapticService hapticService)
        {
            _hapticService = hapticService;
        }
        
        public void Play()
        {
            var currentTime = Time.time;
            if (_limitRate && (currentTime - _lastPlayTime < _limitRateInterval))
            {
                return;
            }
            _lastPlayTime = currentTime;
            _hapticService.PlayVibration(_isDefaultVibration, _specificHapticType);
        }
    }
}
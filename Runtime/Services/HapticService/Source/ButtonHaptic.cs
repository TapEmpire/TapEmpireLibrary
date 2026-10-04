using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services
{
    public class ButtonHaptic : MonoBehaviour
    {
        [SerializeField] private HapticTypes _hapticType = HapticTypes.LightImpact;
        
        private IHapticService _hapticService;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonPress);
        }

        [Inject]
        private void Construct(IHapticService hapticService)
        {
            _hapticService = hapticService;
        }

        private void OnButtonPress()
        {
            _hapticService?.PlayHaptic(_hapticType);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonPress);
        }
    }
}

using MoreMountains.NiceVibrations;

namespace TapEmpire.Services
{
    public interface IHapticService : IService
    {
        bool IsHapticsActive { get; }
        
        void SetHapticsActive(bool active, bool playHapticOnOff);
        void PlayHaptic(HapticTypes type);
        void PlayContinuousHaptic(float intensity, float sharpness, float duration);
        void Vibrate();

        void PlayDefaultVibration();
    }
}

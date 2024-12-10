using MoreMountains.NiceVibrations;

namespace TapEmpire.Services
{
    public interface IHapticService : IService
    {
        bool IsHapticsActive { get; }
        
        void SetHapticsActive(bool active, bool playHapticOnOff);
        void PlayHaptic(HapticTypes type);
        void Vibrate();

        void PlayDefaultVibration();
    }
}

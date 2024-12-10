using MoreMountains.NiceVibrations;

namespace TapEmpire.Services
{
    public static class HapticServiceExtensions
    {
        public static void PlayVibration(this IHapticService self, bool isDefault, HapticTypes specificHapticType = HapticTypes.None)
        {
            if (isDefault)
            {
                self.Vibrate();
            }
            else
            {
                self.PlayHaptic(specificHapticType);
            }
        }
    }
}
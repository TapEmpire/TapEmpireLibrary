namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public static void SetAdsDisabled(this IProgressService self, bool value)
        {
            self.SetBoolProp(ProgressBoolProp.DisableAds, value);
        }
    }
}

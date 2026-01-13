using UnityEngine.Localization;

namespace TapEmpire.Services.Localization
{
    public static class LocalizationUtility
    {
        public static string GetCountryCode(this LocaleIdentifier identifier)
        {
            return identifier.Code.Split('-')[0];
        }
    }
}
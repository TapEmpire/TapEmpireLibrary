using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace TapEmpire.Services.Localization
{
    public static class LocalizationUtility
    {
        public static string GetCountryCode(this LocaleIdentifier identifier)
        {
            return identifier.Code.Split('-')[0];
        }

        public static void CreateOrUpdateEntry(this StringTable table, string entryName, string localizedString)
        {
            var entry = table.GetEntry(entryName);
            if (entry != null)
            {
                entry.Value = localizedString;
            }
            else
            {
                table.AddEntry(entryName, localizedString);
            }
        }
    }
}
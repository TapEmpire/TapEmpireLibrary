using UnityEngine.Localization;

namespace TapEmpire.Services.Localization
{
    public class LocaleModel
    {
        public string ShortName;
        public Locale Locale;
        public string LocalizedString;

        public LocaleModel(string shortName, Locale locale)
        {
            ShortName = shortName;
            Locale = locale;
            LocalizedString = LocalizationService.GetLanguageLocalizedName(shortName);
        }

        public static LocaleModel None => s_none;
        public static string NoneName = "None";

        private static LocaleModel s_none = new ("None", null);
    }
}
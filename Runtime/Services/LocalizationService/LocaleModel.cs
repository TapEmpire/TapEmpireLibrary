using UnityEngine.Localization;

namespace TapEmpire.Services.Localization
{
    public class LocaleModel
    {
        public string ShortName;
        public Locale Locale;

        public LocaleModel(string shortName, Locale locale)
        {
            ShortName = shortName;
            Locale = locale;
        }
    }
}
namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public static string LocaleKey = "Locale";

        public static string GetLocale(this IProgressService self)
        {
            return self.StringValuesDictionary.TryGetValue(LocaleKey, out var value, canUseDefault: false) ? value : "English";
        }

        public static void SetLocale(this IProgressService self, string value)
        {
            self.StringValuesDictionary.SetValue(LocaleKey, value);
        }
    }
}
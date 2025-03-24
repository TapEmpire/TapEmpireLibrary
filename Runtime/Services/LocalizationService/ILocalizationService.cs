using System.Collections.Generic;
using R3;

namespace TapEmpire.Services.Localization
{
    public interface ILocalizationService : IService
    {
        ReadOnlyReactiveProperty<LocaleModel> Locale { get; }
        LocaleModel SelectedLocale { get; }
        List<LocaleModel> GetAvailableLocales();
        void SetSelectedLocale(LocaleModel locale);
    }
}
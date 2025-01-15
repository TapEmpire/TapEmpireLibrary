using System.Collections.Generic;

namespace TapEmpire.Services.Localization
{
    public interface ILocalizationService : IService
    {
        LocaleModel SelectedLocale { get; }
        List<LocaleModel> GetAvailableLocales();
        void SetSelectedLocale(LocaleModel locale);
    }
}
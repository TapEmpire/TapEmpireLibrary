using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zenject;

namespace TapEmpire.Services.Localization
{
    public class LocalizationService : Initializable, ILocalizationService
    {
        public LocaleModel SelectedLocale { get; private set; }

        private IProgressService _progressService;
        private List<LocaleModel> _localeModels = new();
        
        [Inject]
        private void Construct(IProgressService progressService)
        {
            _progressService = progressService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var savedLocale = _progressService.GetLocale();
            var locales = LocalizationSettings.AvailableLocales.Locales;
            _localeModels.Clear();
            foreach (var locale in locales)
            {
                var name = locale.LocaleName.Split(' ')[0];
                var model = new LocaleModel(name, locale);
                _localeModels.Add(model);

                if (name == savedLocale)
                {
                    SelectedLocale = model;
                    LocalizationSettings.SelectedLocale = model.Locale;
                }
            }
            
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            base.OnRelease();
        }

        public List<LocaleModel> GetAvailableLocales()
        {
            return _localeModels;
        }

        public void SetSelectedLocale(LocaleModel locale)
        {
            SelectedLocale = locale;
            LocalizationSettings.SelectedLocale = SelectedLocale.Locale;
            _progressService.SetLocale(locale.ShortName);
        }

        public static string GetLocalizedString(string tableName, string entryName)
        {
            var localizedString = new LocalizedString(tableName, entryName);
            return localizedString.GetLocalizedString();
        }
        
        public static string GetLevelTableName(string levelName)
        {
            return $"Level_{levelName}";
        }
    }
}
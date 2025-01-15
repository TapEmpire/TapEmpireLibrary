using System;
using UnityEngine.Localization;

namespace TapEmpire.Services.Localization
{
    public class LocalizationStringModel : IDisposable
    {
        public System.Action<string> OnStringValueChanged;
        public string CurrentText => _text;

        private LocalizedString _localizedString;
        private string _text;
        private Action<string> _callback;

        public LocalizationStringModel(string tableName, string entryName, Action<string> callback)
        {
            _callback = callback;
            _localizedString = new LocalizedString(tableName, entryName);
            _localizedString.StringChanged += OnChangeLocalizedString;
        }

        private void OnChangeLocalizedString(string value)
        {
            _text = value;
            _callback?.Invoke(_text);
        }

        public void Dispose()
        {
            _localizedString.StringChanged -= OnChangeLocalizedString;
        }
    }
}
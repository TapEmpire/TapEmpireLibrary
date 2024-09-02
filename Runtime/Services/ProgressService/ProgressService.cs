using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using TapEmpire.Services;
using UnityEditor;

namespace TapEmpire.Services
{
    [Serializable]
    public class ProgressService : Initializable, IProgressService
    {
        public ICachedReactiveDictionary<int> IntValuesDictionary { get; private set; }
        public ICachedReactiveDictionary<bool> BoolValuesDictionary { get; private set; }
        public ICachedReactiveDictionary<string> StringValuesDictionary { get; private set; }
        public event Action OnClearProgress;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            IntValuesDictionary = new PlayerPrefsIntReactiveDictionary();
            BoolValuesDictionary = new PlayerPrefsBoolReactiveDictionary();
            StringValuesDictionary = new PlayerPrefsStringReactiveDictionary();

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            IntValuesDictionary?.ClearCache();
            BoolValuesDictionary?.ClearCache();
            StringValuesDictionary?.ClearCache();
            base.OnRelease();
        }

        public void ClearProgress()
        {
            IntValuesDictionary?.ClearCache();
            BoolValuesDictionary?.ClearCache();
            StringValuesDictionary?.ClearCache();
            PlayerPrefs.DeleteAll();
            OnClearProgress?.Invoke();
        }

        public bool TryLoad<T>(string key, out T item)
        {
            item = default;
            var obj = PlayerPrefs.GetString(key, string.Empty);
            if (obj == string.Empty)
                return false;
            item = JsonConvert.DeserializeObject<T>(obj);
            return true;
        }

        public void Save<T>(string key, T obj)
        {
            var serializeObject = JsonConvert.SerializeObject(obj);
            PlayerPrefs.SetString(key, serializeObject);
        }
    }
}

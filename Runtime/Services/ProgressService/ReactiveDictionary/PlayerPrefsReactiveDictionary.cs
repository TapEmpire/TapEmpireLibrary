using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public abstract class PlayerPrefsReactiveDictionary<TValue> : ICachedReactiveDictionary<TValue>
    {
        private readonly Dictionary<string, TValue> _dictionary = new ();

        public event Action<string, TValue> OnSetValue;
        
        protected abstract TValue GetValueFromPrefs(string key);
        
        protected abstract void SetValueToPrefs(string key, TValue value);

        public void ClearCache()
        {
            var keys = new List<string>(_dictionary.Keys);
            foreach (var key in keys)
            {
                SetCacheValue(key, default(TValue));
            }
            _dictionary.Clear();
        }

        private void SetCacheValue(string key, TValue value)
        {
            _dictionary[key] = value;
            OnSetValue?.Invoke(key, value);
        }
        
        public void SetValue(string key, TValue value, bool save = true)
        {
            SetValueToPrefs(key, value);
            if (save)
            {
                PlayerPrefs.Save();
            }

            SetCacheValue(key, value);
        }
        
        public bool TryGetValue(string key, out TValue value, bool canUseCached = true, bool canUseDefault = true, bool setNotCached = true)
        {
            if (canUseCached && _dictionary.TryGetValue(key, out value))
            {
                return true;
            }
            if (PlayerPrefs.HasKey(key))
            {
                value = GetValueFromPrefs(key);
                if (setNotCached)
                {
                    SetCacheValue(key, value);
                }
                return true;
            }
            value = default;
            return canUseDefault;
        }
    }
}
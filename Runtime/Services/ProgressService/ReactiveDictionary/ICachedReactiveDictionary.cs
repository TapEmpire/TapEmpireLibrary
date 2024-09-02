using System;

namespace TapEmpire.Services
{
    public interface ICachedReactiveDictionary<TValue>
    {
        event Action<string, TValue> OnSetValue;

        void SetValue(string key, TValue value, bool save = true);

        bool TryGetValue(string key, out TValue value, bool canUseCached = true, bool canUseDefault = true, bool setNotCached = true);

        void ClearCache();
    }
}
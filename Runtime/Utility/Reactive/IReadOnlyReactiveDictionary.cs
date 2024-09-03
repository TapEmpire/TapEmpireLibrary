using System;
using System.Collections.Generic;

namespace TapEmpire.Utility
{
    public interface IReadOnlyReactiveDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        bool ContainsKey(TKey key);
        
        bool TryGetValue(TKey key, out TValue value);
        
        event Action<TKey, TValue> OnAdd;

        event Action<TKey> OnRemove;
    }
}
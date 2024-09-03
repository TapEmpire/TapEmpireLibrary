using System;
using System.Collections;
using System.Collections.Generic;

namespace TapEmpire.Utility
{
    public class ReactiveDictionary<TKey, TValue> : IReadOnlyReactiveDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public event Action<TKey, TValue> OnAdd;

        public event Action<TKey> OnRemove;

        public ReactiveDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public ReactiveDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
        
        public void Set(Dictionary<TKey, TValue> dictionary)
        {
            Clear();
            foreach (var (key, value) in dictionary)
            {
                Add(key, value);
            }
        }

        public void Clear()
        {
            foreach (var (key, value) in _dictionary)
            {
                OnRemove?.Invoke(key);
            }    
            _dictionary.Clear();
        }
        
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            OnAdd?.Invoke(key, value);
        }
        
        public void Remove(TKey key)
        {
            _dictionary.Remove(key);
            OnRemove?.Invoke(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
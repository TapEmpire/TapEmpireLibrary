using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace TapEmpire.Utility
{
    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> self, IEnumerable<TKey> keysToRemove)
        {
            foreach (var key in keysToRemove)
            {
                self.Remove(key);
            }
        }

        public static TValue GetFirstOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> addDelegate)
        {
            if (self.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                var newValue = addDelegate.Invoke(key);
                self.Add(key, newValue);
                return newValue;
            }
        }
        
        public static (TKey key, TValue value) GetFirstOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, Func<TValue, bool> getDelegate, Func<(TKey key, TValue value)> addDelegate)
        {
            if (self.TryGetFirst(kvp => getDelegate.Invoke(kvp.Value), out var keyValuePair))
            {
                return (keyValuePair.Key, keyValuePair.Value);
            }
            else
            {
                var keyValueTuple = addDelegate.Invoke();
                self.Add(keyValueTuple.key, keyValueTuple.value);
                return keyValueTuple;
            }
        }

        public static Dictionary<TValue, List<TKey>> Invert<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> self)
        {
            var inverted = new Dictionary<TValue, List<TKey>>();

            foreach (var (key, value) in self)
            {
                inverted.GetFirstOrAdd(value, _ => new List<TKey>()).Add(key);
            }

            return inverted;
        }

        public static void AddIfNone<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (!self.ContainsKey(key))
            {
                self.Add(key, value);
            }
        }

        public static TValue GetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            if (self.TryGetValue(key, out var value))
            {
                self.Remove(key);
                return value;
            }

            return default;
        }
        
        public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> self, Func<TValue, bool> conditionToRemove)
        {
            using (ListScope<TKey>.Create(out var keysToRemove))
            {
                foreach (var (key, value) in self)
                {
                    if (conditionToRemove.Invoke(value))
                    {
                        keysToRemove.Add(key);
                    }
                }
                self.RemoveAll(keysToRemove);
            }
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            return self.TryGetValue(key, out var value) ? value : default(TValue);
        }

        public static KeyValuePair<TKey, TValue> GetValueOrFirst<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            return self.TryGetValue(key, out var value) ? KeyValuePair.Create(key, value) : self.First();
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> FindAll<TKey, TValue>(this Dictionary<TKey, TValue> self, Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            foreach (var entry in self)
            {
                if (predicate(entry))
                {
                    yield return entry;
                }
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;

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
    }
}
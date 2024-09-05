using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Utility
{
    [Serializable]
    public sealed partial class SerializableReferencesDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [HideInInspector]
        [SerializeField]
        private Entry[] _entries = Array.Empty<Entry>();

        #region ISerializationCallbackReceiver

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var entry in _entries)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public void OnBeforeSerialize()
        {
            using (ListScope<Entry>.Create(out var entries))
            {
                foreach (var (key, value) in this)
                {
                    entries.Add(new Entry
                    {
                        Key = key,
                        Value = value
                    });
                }

#if UNITY_EDITOR
                if (typeof(TKey).IsEnum)
                {
                    entries.Sort((x, y) => Comparer<int>.Default.Compare((int)(object)x.Key, (int)(object)y.Key));
                }
                else if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    entries.Sort((x, y) => Comparer<TKey>.Default.Compare(x.Key, y.Key));
                }
#endif

                _entries = entries.ToArray();
            }
        }

        #endregion
        
        public void SetFromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            Clear();

            foreach (var kvp in dictionary)
            {
                this[kvp.Key] = kvp.Value;
            }

            UpdateSerializedEntries();
        }

        private void UpdateSerializedEntries()
        {
            using (ListScope<Entry>.Create(out var entries))
            {
                foreach (var (key, value) in this)
                {
                    entries.Add(new Entry
                    {
                        Key = key,
                        Value = value
                    });
                }

#if UNITY_EDITOR
                if (typeof(TKey).IsEnum)
                {
                    entries.Sort((x, y) => Comparer<int>.Default.Compare((int)(object)x.Key, (int)(object)y.Key));
                }
                else if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    entries.Sort((x, y) => Comparer<TKey>.Default.Compare(x.Key, y.Key));
                }
#endif

                _entries = entries.ToArray();
            }
        }
    }

}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Utility
{
    [Serializable]
    public sealed class SerializableReferencesDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();

        [SerializeReference]
        private List<TValue> _values = new List<TValue>();

        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count)
                throw new InvalidOperationException($"There are {_keys.Count} keys and {_values.Count} values after deserialization. Make sure that both key and value types are serializable.");

            for (int i = 0; i < _keys.Count; i++)
            {
                this[_keys[i]] = _values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }
    }

}
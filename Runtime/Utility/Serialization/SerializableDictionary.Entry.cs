using System;
using UnityEngine;

public sealed partial class SerializableDictionary<TKey, TValue>
{
    [Serializable]
    private struct Entry
    {
        [SerializeField]
        private TKey _key;

        [SerializeField]
        private TValue _value;

        public TKey Key
        {
            get => _key;
            set => _key = value;
        }

        public TValue Value
        {
            get => _value;
            set => _value = value;
        }
    }
}
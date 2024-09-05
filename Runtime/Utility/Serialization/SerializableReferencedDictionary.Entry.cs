using System;
using UnityEngine;

namespace TapEmpire.Utility
{
    public sealed partial class SerializableReferencesDictionary<TKey, TValue>
    {
        [Serializable]
        private struct Entry
        {
            [SerializeField]
            private TKey _key;

            [SerializeReference]
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
}
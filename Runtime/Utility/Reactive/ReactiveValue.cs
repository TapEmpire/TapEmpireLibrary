using System;

namespace TapEmpire.Utility
{
    public class ReactiveValue<TValue>
    {
        private TValue _value;

        public event Action<TValue> OnSet;

        public ReactiveValue(TValue value)
        {
            _value = value;
        }
        
        public TValue Value => _value;
        
        public void Set(TValue value)
        {
            _value = value;
            OnSet?.Invoke(value);
        }
    }
}
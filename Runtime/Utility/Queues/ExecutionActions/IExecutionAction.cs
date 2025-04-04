using System;
using R3;

namespace TapEmpire.Utility
{
    public interface IExecutionAction<T> : IDisposable where T : Enum
    {
        ReactiveProperty<bool> OnDone { get; }

        void Reinitialize();
        void Execute(T flow = default);
        IExecutionAction<T> AddCallbackOnce(System.Action callback);
    }

    public class ExecutionAction<T> : IExecutionAction<T> where T : Enum
    {
        public ReactiveProperty<bool> OnDone { get; private set; } = new(false);

        private System.Action _callback = null;

        public virtual void Execute(T flow = default)
        {
            MarkComplete();
        }

        public virtual void Reinitialize()
        {
            OnDone.Value = false;
        }

        public IExecutionAction<T> AddCallbackOnce(System.Action callback)
        {
            _callback = callback;
            return this;
        }

        public virtual void Dispose() { }

        protected virtual void MarkComplete()
        {
            OnDone.Value = true;
            _callback?.Invoke();
            _callback = null;
        }

        public static CompositeExecutionAction<T> Composite(params IExecutionAction<T>[] actions)
        {
            return new CompositeExecutionAction<T>(actions);
        }
    }
}
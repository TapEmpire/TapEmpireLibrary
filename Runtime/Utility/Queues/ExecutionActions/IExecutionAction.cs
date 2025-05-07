using System;
using R3;

namespace TapEmpire.Utility
{
    public enum ExecutionState
    {
        NotDone,
        False,
        True
    }

    public interface IExecutionAction<T> : IDisposable where T : Enum
    {
        ReactiveProperty<ExecutionState> OnDone { get; }

        void Reinitialize();
        void Execute(ExecutionState state, T flow = default);
        IExecutionAction<T> AddCallbackOnce(System.Action callback);
    }

    public class ExecutionAction<T> : IExecutionAction<T> where T : Enum
    {
        public ReactiveProperty<ExecutionState> OnDone { get; private set; } = new(ExecutionState.NotDone);

        private System.Action _callback = null;

        public virtual void Execute(ExecutionState state = default, T flow = default)
        {
            MarkComplete();
        }

        public virtual void Reinitialize()
        {
            OnDone.Value = ExecutionState.NotDone;
        }

        public IExecutionAction<T> AddCallbackOnce(System.Action callback)
        {
            _callback = callback;
            return this;
        }

        public virtual void Dispose() { }

        protected void MarkComplete()
        {
            MarkComplete(true);
        }

        protected virtual void MarkComplete(bool result)
        {
            OnDone.Value = result ? ExecutionState.True : ExecutionState.False;
            _callback?.Invoke();
            _callback = null;
        }

        public static CompositeExecutionAction<T> Composite(params IExecutionAction<T>[] actions)
        {
            return new CompositeExecutionAction<T>(actions);
        }
    }
}
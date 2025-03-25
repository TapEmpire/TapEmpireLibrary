using System;
using R3;

namespace TapEmpire.Utility
{
    public interface IExecutionAction : IDisposable
    {
        ReactiveProperty<bool> OnDone { get; }

        void Reinitialize();
        void Execute();
        IExecutionAction AddCallbackOnce(System.Action callback);
    }

    public class ExecutionAction : IExecutionAction
    {
        public ReactiveProperty<bool> OnDone { get; private set; } = new(false);

        private System.Action _callback = null;

        public virtual void Execute()
        {
            MarkComplete();
        }

        public virtual void Reinitialize()
        {
            OnDone.Value = false;
        }

        public IExecutionAction AddCallbackOnce(System.Action callback)
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

        public static CompositeExecutionAction Composite(params IExecutionAction[] actions)
        {
            return new CompositeExecutionAction(actions);
        }
    }
}
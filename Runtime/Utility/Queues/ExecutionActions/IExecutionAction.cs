using R3;

namespace TapEmpire.Utility
{
    public interface IExecutionAction
    {
        ReactiveProperty<bool> OnDone { get; }

        void Reinitialize();
        void Execute();
    }

    public class ExecutionAction : IExecutionAction
    {
        public ReactiveProperty<bool> OnDone { get; private set; } = new(false);

        public virtual void Execute()
        {
            MarkComplete();
        }

        public virtual void Reinitialize()
        {
            OnDone.Value = false;
        }

        protected virtual void MarkComplete()
        {
            OnDone.Value = true;
        }
    }
}
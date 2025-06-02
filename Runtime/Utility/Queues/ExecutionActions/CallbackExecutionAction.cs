using System;

namespace TapEmpire.Utility
{
    public class CallbackExecutionAction<T> : ExecutionAction<T> where T : Enum
    {
        private System.Action<T> _action = null;

        public CallbackExecutionAction(System.Action<T> action)
        {
            _action = action;
        }

        public override void Execute(ExecutionState state, T flow = default)
        {
            _action?.Invoke(flow);
            MarkComplete();
        }

        public override void Dispose()
        {
            _action = null;
            base.Dispose();
        }
    }
}
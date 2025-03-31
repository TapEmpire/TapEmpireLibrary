using System;

namespace TapEmpire.Utility
{
    public class CallbackExecutionAction : ExecutionAction
    {
        private System.Action _action = null;

        public CallbackExecutionAction(System.Action action)
        {
            _action = action;
        }

        public override void Execute()
        {
            _action?.Invoke();
            MarkComplete();
        }

        public override void Dispose()
        {
            _action = null;
            base.Dispose();
        }
    }
}
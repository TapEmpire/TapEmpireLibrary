using TapEmpire.Services;
using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public class IapExecutionAction : ExecutionAction<FlowAction>
    {
        private IIapService _iapService;
        private int _level;

        public IapExecutionAction(IIapService iapService, int level)
        {
            _iapService = iapService;
            _level = level;
        }

        public override void Execute(ExecutionState state, FlowAction flow)
        {
            if (flow == FlowAction.Restart)
            {
                MarkComplete();
                return;
            }
            
            _iapService.ShowOnLevel(_level + 1, MarkComplete);
        }
    }
}
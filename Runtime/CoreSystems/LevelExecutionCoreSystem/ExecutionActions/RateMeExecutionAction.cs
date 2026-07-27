using Cysharp.Threading.Tasks;
using TapEmpire.Services;
using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public class RateMeExecutionAction : ExecutionAction<FlowAction>
    {
        private IRateMeService _rateMeService;
        private int _level;

        public RateMeExecutionAction(IRateMeService rateMeService, int level)
        {
            _rateMeService = rateMeService;
            _level = level;
        }

        public override void Execute(ExecutionState state, FlowAction flow)
        {
            _rateMeService.RateOnLevel(_level + 1).ContinueWith(MarkComplete).Forget();
        }
    }
}
using System.Threading;
using TapEmpire.Services;
using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public class NetworkExecutionAction : ExecutionAction<FlowAction>
    {
        private INetworkService _networkService;

        public NetworkExecutionAction(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public override async void Execute(ExecutionState state, FlowAction flow)
        {
            await _networkService.WaitNetworkAsync(CancellationToken.None);
            MarkComplete();
        }
    }
}
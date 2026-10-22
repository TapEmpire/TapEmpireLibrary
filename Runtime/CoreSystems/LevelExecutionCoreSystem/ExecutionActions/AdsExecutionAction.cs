using TapEmpire.Services;
using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public class AdsExecutionAction : ExecutionAction<FlowAction>
    {
        private IAdsService _adsService;
        private int _level;
        private bool _shouldSkipAd;

        public AdsExecutionAction(IAdsService adsService, int level, bool shouldSkipAd)
        {
            _adsService = adsService;
            _level = level;
            _shouldSkipAd = shouldSkipAd;
        }

        public override void Execute(ExecutionState state, FlowAction flow)
        {
            if (_shouldSkipAd)
            {
                MarkComplete();
                return;
            }
            
            _adsService.ShowInterstitial(_level, string.Empty, MarkComplete);
        }
    }
}
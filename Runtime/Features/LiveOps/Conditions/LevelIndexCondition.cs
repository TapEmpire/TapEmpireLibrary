namespace TapEmpire.Services.LiveOps
{
    public class LevelIndexCondition : ICondition
    {
        private readonly int _minStartLevelIndex;
        private readonly IProgressService _progressService;

        public LevelIndexCondition(int minStartLevelIndex, IProgressService progressService)
        {
            _minStartLevelIndex = minStartLevelIndex;
            _progressService = progressService;
        }

        public bool Evaluate()
        {
            return _progressService.GetLevelProgress() >= _minStartLevelIndex;
        }
    }
}

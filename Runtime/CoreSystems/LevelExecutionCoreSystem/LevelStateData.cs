using R3;

namespace TapEmpire.CoreSystems
{
    public class LevelStateData
    {
        public readonly Subject<LevelStateData> OnDataChanged = new Subject<LevelStateData>();

        public int StepsDone { get; private set; } = 0;
        public int StepsTotal { get; private set; } = 0;
        public LevelState LevelState { get; private set; } = LevelState.Active;

        public bool IsOver => StepsDone >= StepsTotal;

        public void SetStepsTotal(int value)
        {
            StepsTotal = value;
        }

        public void Initialize(int stepsTotal, int stepsDone = 0)
        {
            StepsDone = stepsDone;
            StepsTotal = stepsTotal;
        }

        public int AddStepDone()
        {
            ++StepsDone;
            OnDataChanged.OnNext(this);

            return StepsDone;
        }

        public void SetState(LevelState state)
        {
            LevelState = state;
            OnDataChanged.OnNext(this);
        }
    }
}
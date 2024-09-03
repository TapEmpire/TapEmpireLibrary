namespace TapEmpire.Utility
{
    public class ConditionBarrier
    {
        int numberOfConditions = 0;
        int numberOfFullfilled = 0;

        System.Action callback = null;

        public ConditionBarrier(int numberOfConditions, System.Action callback)
        {
            this.numberOfConditions = numberOfConditions;
            this.callback = callback;
        }

        public void Done()
        {
            // Debug.LogError($"Number of DONE elements: {numberOfFullfilled}");
            if (++numberOfFullfilled >= numberOfConditions)
            {
                callback?.Invoke();
                callback = null;
            }
        }
    }
}
namespace TapEmpire.Utility
{
    public interface ITicksContainer
    {
        void InitializeTicks<T>(T[] targets) where T : class;
        
        void InitializeTicks<T>(T target) where T : class;

        void ReleaseTicks<T>(T target) where T : class;
        
        void ReleaseTicks<T>(T[] targets) where T : class;

    }
}
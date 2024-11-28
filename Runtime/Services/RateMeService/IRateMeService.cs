namespace TapEmpire.Services
{
    public interface IRateMeService : IService
    {
        bool HasRated { get; }

        void RateOnLevel(int level); // Human level, not levelIndex.
    }
}
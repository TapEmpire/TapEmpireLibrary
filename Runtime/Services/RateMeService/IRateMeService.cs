using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IRateMeService : IService
    {
        bool HasRated { get; }

        UniTask Rate();
        UniTask RateOnLevel(int level); // Human level, not levelIndex.
        bool ShouldRate(int level);
        bool IsLevelEligibleForRateMe(int level);
        bool IsAccept { get; set; }
    }
}
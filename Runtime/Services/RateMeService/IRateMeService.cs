using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IRateMeService : IService
    {
        bool HasRated { get; }

        UniTask Rate();
        UniTask RateOnLevel(int level); // Human level, not levelIndex.
        bool IsNeedRated(int level);
    }
}
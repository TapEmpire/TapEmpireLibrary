using TapEmpire.Level;
using TapEmpire.Settings;

namespace TapEmpire.Services
{
    public interface IGameService : IService
    {
        GameSettings GameSettings { get; }
        LevelsTable LevelsTable { get; }

        LevelSettings GetCurrentLevel();
    }
}

using TapEmpire.Services;

namespace TapEmpire.Game
{
    public interface ISceneSelector
    {
        SceneName GetNextScene();
    }
}
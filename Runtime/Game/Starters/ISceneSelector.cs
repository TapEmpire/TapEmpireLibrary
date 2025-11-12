using TapEmpire.Services;

namespace TapEmpire.Game
{
    public interface ISceneSelector
    {
        (SceneName, bool) GetNextScene();
    }
}
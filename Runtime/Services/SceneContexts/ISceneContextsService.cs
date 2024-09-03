using System;
using Zenject;

namespace TapEmpire.Services
{
    public interface ISceneContextsService : IService
    {
        event Action<string, SceneContext> OnSceneContextInstalled; 
        
        void AddInstalledSceneContext(string id, SceneContext sceneContext);
    }
}
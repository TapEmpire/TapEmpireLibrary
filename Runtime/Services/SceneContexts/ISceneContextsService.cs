using System;
using Zenject;
using R3;

namespace TapEmpire.Services
{
    public interface ISceneContextsService : IService
    {
        event Action<string, SceneContext> OnSceneContextInstalled;
        public Observable<(string, SceneContext)> OnSceneContextInstalledR3 { get; }
        
        void AddInstalledSceneContext(string id, SceneContext sceneContext);
    }
}
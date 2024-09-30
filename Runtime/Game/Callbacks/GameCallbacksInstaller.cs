using System;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace TapEmpireLibrary.Game
{
    [Serializable]
    public class GameCallbacksInstaller : MonoInstaller<GameCallbacksInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ITicksContainer>().FromInstance(new TicksContainer());

            var monoBehaviourCallbacksGameObject = new GameObject(nameof(MonoBehaviourCallbacks));
            var monoBehaviourCallbacks = monoBehaviourCallbacksGameObject.AddComponent<MonoBehaviourCallbacks>();
            DontDestroyOnLoad(monoBehaviourCallbacksGameObject);
            
            Container.Bind<IGameEventsContainer>().FromInstance(monoBehaviourCallbacks);
        }
    }
}
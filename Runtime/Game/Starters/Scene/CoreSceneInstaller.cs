using UnityEngine;
using Zenject;

namespace TapEmpire.Level
{
    public class CoreSceneInstaller : MonoInstaller<CoreSceneInstaller>
    {
        [SerializeField]
        private CoreSceneReferences _coreSceneReferences;
        
        public override void InstallBindings()
        {
            Container.Bind<ICoreSceneReferences>().FromInstance(_coreSceneReferences);
        }
    }
}
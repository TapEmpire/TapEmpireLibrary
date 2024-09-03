using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class SceneContextsService : Initializable, ISceneContextsService
    {
        private Dictionary<string, SceneContext> _sceneContexts = new ();
        
        public event Action<string, SceneContext> OnSceneContextInstalled;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _sceneContexts = new Dictionary<string, SceneContext>();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _sceneContexts.Clear();
            base.OnRelease();
        }

        public void AddInstalledSceneContext(string id, SceneContext sceneContext)
        {
            if (_sceneContexts.ContainsKey(id))
            {
                _sceneContexts.Remove(id);
            }
            _sceneContexts.Add(id, sceneContext);
            OnSceneContextInstalled?.Invoke(id, sceneContext);
        }
    }
}
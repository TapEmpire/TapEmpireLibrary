using System;
using R3;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.UI
{
    public class DebugPage : MonoBehaviour, IDisposable
    {
        [SerializeField] private DebugComponent[] _components;

        protected readonly CompositeDisposable _disposables = new();

        public virtual IDisposable Initialize()
        {
            _components.ForEach(component => component.Initialize().AddTo(_disposables));
            return this;
        }

        public virtual void Dispose() => _disposables.Dispose();

        public virtual void OnOpenDebug() => _components.ForEach(component => component.Read());

        public virtual void OnCloseDebug() { }

        [Button]
        private void CollectComponents()
        {
            _components = GetComponentsInChildren<DebugComponent>(true);
        }
    }
}

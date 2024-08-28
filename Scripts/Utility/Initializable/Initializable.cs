using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Services
{
    public abstract class Initializable : IInitializable
    {
        [NonSerialized]
        private CancellationTokenSource _cancellationTokenSource;
        
        [NonSerialized]
        private bool _initialized;

        bool IInitializable.Initialized => _initialized;

        async UniTask IInitializable.InitializeAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);

            await OnInitializeAsync(cancellationToken);

            _initialized = true;
        }

        void IInitializable.Release()
        {
            OnRelease();

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            
            _initialized = false;
        }

        protected virtual UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void OnRelease()
        {
        }
    }
}
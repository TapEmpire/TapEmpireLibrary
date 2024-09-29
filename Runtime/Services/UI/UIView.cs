using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.UI
{
    public abstract class UIView : MonoBehaviour
    {
        public IUIViewModel Model;
        
        public virtual async UniTask OpenAsync(CancellationToken cancellationToken)
        {
            await OnOpenAsync(cancellationToken);
        }

        protected virtual UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
        
        public virtual async UniTask CloseAsync(CancellationToken cancellationToken)
        {
            await OnCloseAsync(cancellationToken);
        }
        
        protected virtual UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }

    public abstract class UIView<T> : UIView
        where T : IUIViewModel
    {
        protected T DerivedModel => (T)Model;
    }
}
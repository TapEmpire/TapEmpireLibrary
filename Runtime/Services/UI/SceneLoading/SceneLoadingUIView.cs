using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class SceneLoadingUIView : UIView<SceneLoadingUIViewModel>
    {   
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            DerivedModel.SetProgressCallback += OnSetProgressCallback;
            return base.OnOpenAsync(cancellationToken);
        }
        
        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            DerivedModel.SetProgressCallback -= OnSetProgressCallback;
            return base.OnCloseAsync(cancellationToken);
        }

        protected virtual void OnSetProgressCallback(float progress, float duration)
        {
        }
    }
}
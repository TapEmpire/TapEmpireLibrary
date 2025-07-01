using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.UI
{
    public class CustomCursorUIView : UIView<CustomCursorUIViewModel>, IInjectable
    {
        [SerializeField] private RectTransform _imageTransform;

        private RectTransform _canvasTransform;
        private bool _isRunning;
        
        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            var canvas = GetComponentInParent<Canvas>();
            _canvasTransform = (RectTransform)canvas.transform;

            _isRunning = true;
            
            return base.OpenAsync(cancellationToken);
        }

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            return base.OnOpenAsync(cancellationToken);
        }

        private void Update()
        {
            if (!_isRunning) return;
            
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvasTransform.transform,
                Input.mousePosition,
                null,
                out position
            );

            _imageTransform.localPosition = position;
        }
    }
}
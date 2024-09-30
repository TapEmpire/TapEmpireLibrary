﻿using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class NoInternetUIView : UIView<NoInternetUIViewModel>, IFadeAbleView
    {
        [SerializeField]
        private Transform _bounceRoot;

        [SerializeField]
        private Button _button;

        [SerializeField]
        private DoTweenExplodeBounceData _bounceData;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _button.onClick.AddListener(OnClickButton);
            return base.OnOpenAsync(cancellationToken);
        }
        
        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _button.onClick.RemoveListener(OnClickButton);
            return base.OnCloseAsync(cancellationToken);
        }

        private void OnClickButton()
        {
            DoTweenUtility.ExplodeBounce(_bounceRoot, _bounceData);
        }
    }
}
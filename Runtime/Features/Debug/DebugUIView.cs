using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public abstract class DebugUIView<TViewModel> : UIView<TViewModel>, IInjectable, IDebugUIView
        where TViewModel : IUIViewModel
    {
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _contentContainer;
        [SerializeField] private List<DebugPageEntry> _pages;

        protected readonly CompositeDisposable _disposables = new();

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _openButton.onClick.Subscribe(() => SetContentVisible(true)).AddTo(_disposables);
            _closeButton.onClick.Subscribe(() => SetContentVisible(false)).AddTo(_disposables);

            _pages.ForEach((entry, index) => entry.Button.onClick.Subscribe(() => ShowPage(index)).AddTo(_disposables));
            _pages.ForEach(entry => entry.Page.Initialize(this).AddTo(_disposables));

            ShowPage(0);
            return base.OpenAsync(cancellationToken);
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return base.CloseAsync(cancellationToken);
        }

        public void SetContentVisible(bool isVisible)
        {
            _contentContainer.SetActive(isVisible);
            _pages.ForEach(entry => (isVisible ? (Action)entry.Page.OnOpenDebug : entry.Page.OnCloseDebug)());
        }

        public void ShowPage(int index)
        {
            _pages.ForEach((entry, entryIndex) => entry.Page.gameObject.SetActive(entryIndex == index));
        }

        [Button("@_contentContainer.activeSelf ? \"Close container\" : \"Open container\"")]
        private void ToggleVisibility()
        {
            SetContentVisible(!_contentContainer.activeSelf);
        }

        [Button("Open page")]
        private void OpenPage(int index)
        {
            ShowPage(index);
        }
    }

    [Serializable]
    public struct DebugPageEntry
    {
        public Button Button;
        public DebugPage Page;
    }
}

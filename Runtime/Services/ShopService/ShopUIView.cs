using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services.Shop
{
    public class ShopUIView : UIView<ShopUIViewModel>, IInjectable
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _settingsButton = null;
        [SerializeField] private Transform _content;
        [SerializeField] private int _minBottomOffset;
        [SerializeField] private int _maxBottomOffset;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

        private List<ShopSection> _shopSections = new();
        private CompositeDisposable _disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _closeButton.gameObject.SetActive(DerivedModel.HasCloseButton);
            if (DerivedModel.HasCloseButton)
            {
                _closeButton.onClick.Subscribe(DerivedModel.OnClosePressed).AddTo(_disposables);
            }

            if (_settingsButton != null)
            {
                _settingsButton.gameObject.SetActive(!DerivedModel.HasCloseButton);
                if (!DerivedModel.HasCloseButton)
                {
                    _settingsButton.onClick.Subscribe(() => DerivedModel.OnSettingsPressed?.Invoke()).AddTo(_disposables);
                }
            }

            if (_verticalLayoutGroup != null)
            {
                _verticalLayoutGroup.padding.bottom = DerivedModel.HasBottomOffset ? _maxBottomOffset : _minBottomOffset;
            }

            CreateSections(DerivedModel.ShopSettings);

            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _shopSections.Clear();
            _disposables.Dispose();

            return base.OnCloseAsync(cancellationToken);
        }

        private void CreateSections(ShopSettings settings)
        {
            foreach (var sectionData in settings.Sections)
            {
                var section = GameObject.Instantiate(sectionData.SectionPrefab, _content);
                section.Initialize(DerivedModel.DiContainer, sectionData);
                _shopSections.Add(section);
            }
        }
    }
}

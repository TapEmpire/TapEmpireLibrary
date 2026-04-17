using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TapEmpire.Services.LiveOps
{
    [System.Serializable]
    public struct StepInfo
    {
        public GameObject Screen;
        public Image Dot;
        public LocalizedString ButtonText;
    }

    public class LiveOpsTutorialView : UIView<LiveOpsTutorialViewModel>, IInjectable
    {
        [SerializeField] private List<StepInfo> _steps = new();
        [SerializeField] private Button _nextButton = null;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _chosenColor = Color.white;
        [SerializeField] private LocalizeStringEvent _buttonText = null;

        private int _stepIndex = 0;
        private CompositeDisposable _disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _nextButton.onClick.Subscribe(OnNextPressed).AddTo(_disposables);
            SetCurrentStep();

            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();

            return base.OnCloseAsync(cancellationToken);
        }

        private void OnNextPressed()
        {
            if (++_stepIndex < _steps.Count)
            {
                SetCurrentStep();
                return;
            }

            DerivedModel.OnNextPressed();
        }

        private void SetCurrentStep()
        {
            _steps.ForEach(step =>
            {
                step.Screen.SetActive(false);
                step.Dot.color = _defaultColor;
            });

            var step = _steps[_stepIndex];

            step.Screen.SetActive(true);
            step.Dot.color = _chosenColor;

            _buttonText.StringReference = step.ButtonText;
        }
    }
}
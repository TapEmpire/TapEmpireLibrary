using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class DefaultMoveToggleUIView : ToggleUIView
    {
        [SerializeField]
        private Button _toggleButton;

        [Header("Color")]
        [SerializeField]
        private Image _toggleImage;

        [SerializeField]
        private Color _onColor;

        [SerializeField]
        private Color _offColor;
        
        [Header("Toggle move")]
        [SerializeField]
        private RectTransform _toggleTransform;

        [SerializeField]
        private RectTransform _turnOnPoint;

        [SerializeField]
        private RectTransform _turnOffPoint;

        [Header("Animation")]
        [SerializeField]
        private float _duration = 0.2f;

        [SerializeField]
        private Ease _ease = Ease.InOutSine;
        
        private bool _state;
        private bool _isAnimating;
        private Tween _tween;

        private Action<bool> _onSetState;
        
        public override void Initialize(Action<bool> onSetState, bool initialState)
        {
            _toggleButton.onClick.AddListener(OnToggleClick);
            _onSetState = onSetState;
            ChangeState(initialState, true);
        }

        public override void Release()
        {
            _toggleButton.onClick.RemoveListener(OnToggleClick);
            _onSetState = null;
            StopAnimation();
        }
        
        private void OnToggleClick()
        {
            if (_isAnimating)
            {
                return;
            }
            ChangeState(!_state, false);
        }
        
        private void StopAnimation(bool complete = true)
        {
            if (!_isAnimating)
            {
                return;
            }
            _tween?.Kill(complete);
            _tween = null;
        }
        
        [Button]
        private void ChangeState(bool state, bool force)
        {
            if (force)
            {
                _state = state;
                _toggleTransform.position = _state ? _turnOnPoint.position : _turnOffPoint.position;
                _toggleImage.color = _state ? _onColor : _offColor;
                _onSetState?.Invoke(_state);
            }
            else
            {
                if (_state == state)
                {
                    return;
                }
                _state = state;
                _onSetState?.Invoke(_state);
                StopAnimation();
                _isAnimating = true;

                _tween = DOTween.Sequence()
                    .Join(_toggleTransform.DOMove(state ? _turnOnPoint.position : _turnOffPoint.position, _duration).SetEase(_ease))
                    .Join(_toggleImage.DOColor(state ? _onColor : _offColor, _duration))
                    .OnComplete(() => _isAnimating = false);
            }
        }
    }
}

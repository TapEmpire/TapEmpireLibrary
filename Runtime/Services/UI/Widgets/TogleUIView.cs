using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class ToggleUIView : MonoBehaviour
    {
        [SerializeField]
        private Image _turnOnFillImage;

        [SerializeField]
        private Button _toggleButton;
        
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
        private Tween _moveTween;
        private Tween _fillTween;

        private Action<bool> _onSetState;
        
        public void Initialize(Action<bool> onSetState, bool initialState)
        {
            _toggleButton.onClick.AddListener(OnToggleClick);
            _onSetState = onSetState;
            ChangeState(initialState, true);
        }

        public void Release()
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
            _moveTween?.Kill(complete);
            _moveTween = null;
            _fillTween?.Kill(complete);
            _fillTween = null;
        }
        
        private void ChangeState(bool state, bool force)
        {
            if (force)
            {
                _state = state;
                _toggleTransform.position = _state ? _turnOnPoint.position : _turnOffPoint.position;
                _turnOnFillImage.fillAmount = _state ? 1 : 0;
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
                _fillTween = _turnOnFillImage
                    .DOFillAmount(_state ? 1 : 0, _duration)
                    .SetEase(_ease)
                    .OnComplete(() => _isAnimating = false);
                _moveTween = _toggleTransform
                    .DOMove(state ? _turnOnPoint.position : _turnOffPoint.position, _duration)
                    .SetEase(_ease)
                    .OnComplete(() => _isAnimating = false);
            }
        }
    }
}
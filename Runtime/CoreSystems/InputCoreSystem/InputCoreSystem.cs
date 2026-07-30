using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class InputCoreSystem : Initializable, IInputCoreSystem, ITickable
    {
        [field: SerializeField] public InputMode InputMode { get; private set; } = InputMode.Drag;

        public ReactiveProperty<bool> BlockModeProperty { get; private set; }

        public Observable<Vector2> OnScreenInputStart => _onScreenInputStart;
        public Observable<Vector2> OnScreenInputEnd => _onScreenInputEnd;
        public Observable<Vector2> OnScreenTapEnd => _onScreenTapEnd;

        public bool IsInputStart { get; private set; }
        public bool IsInputEnd { get; private set; }
        public bool IsInputHold { get; private set; }

        public bool IsSimulated { get; set; } = false;
        public Vector2 SimulatedPosition { get; set; } = Vector2.zero;

        private readonly Subject<Vector2> _onScreenInputStart = new();
        private readonly Subject<Vector2> _onScreenInputEnd = new();
        private readonly Subject<Vector2> _onScreenTapEnd = new();

        private float _maxTapDuration = 0.2f;
        private float _maxSqrTapMovement = 100.0f;
        private float _touchStartTime = 0.0f;
        private Vector2 _inputStartPosition = Vector2.zero;

        public Vector2 InputPosition
        {
            get
            {
                if (IsSimulated)
                {
                    return SimulatedPosition;
                }
                if (Input.touchSupported)
                {
                    return Input.touchCount <= 0 ? default : Input.GetTouch(0).position;
                }
                return Input.mousePosition;
            }
        }

        public void StartInput(Vector2 position, bool withPress = true)
        {
            if (IsSimulated)
            {
                SimulatedPosition = position;

                if (withPress)
                {
                    _onScreenInputStart.OnNext(position);
                }
            }
        }

        public void EndInput(Vector2 position)
        {
            if (IsSimulated)
            {
                _onScreenInputEnd.OnNext(position);
            }
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            BlockModeProperty = new ReactiveProperty<bool>(false);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _onScreenInputStart.Dispose();
            _onScreenInputEnd.Dispose();
            _onScreenTapEnd.Dispose();
            BlockModeProperty?.Dispose();
        }

        public void Tick()
        {
            if (BlockModeProperty.Value || IsSimulated)
            {
                IsInputStart = false;
                IsInputEnd = false;
                IsInputHold = false;
                return;
            }

            if (TryGetPrimaryInputScreenPosition(out var inputScreenPosition, out var touchPhase))
            {
                switch (touchPhase)
                {
                    case TouchPhase.Began:
                        _onScreenInputStart.OnNext(inputScreenPosition);
                        IsInputStart = true;
                        IsInputEnd = false;
                        IsInputHold = false;
                        SaveTouch(inputScreenPosition);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        IsInputHold = true;
                        IsInputStart = false;
                        IsInputEnd = false;
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        _onScreenInputEnd.OnNext(inputScreenPosition);
                        IsInputEnd = true;
                        IsInputStart = false;
                        IsInputHold = false;
                        if (InputMode.HasFlag(InputMode.Tap))
                        {
                            CheckTouch(inputScreenPosition);
                        }
                        break;

                    default:
                        IsInputStart = false;
                        IsInputEnd = false;
                        IsInputHold = false;
                        break;
                }
            }
            else
            {
                IsInputStart = false;
                IsInputEnd = false;
                IsInputHold = false;
            }
        }

        private bool TryGetPrimaryInputScreenPosition(out Vector2 screenPosition, out TouchPhase touchPhase)
        {
            if (Input.touchSupported)
            {
                if (Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);
                    screenPosition = touch.position;
                    touchPhase = touch.phase;
                    return true;
                }
            }
            else
            {
                screenPosition = Input.mousePosition;
                touchPhase = Input.GetMouseButtonDown(0)
                    ? TouchPhase.Began
                    : Input.GetMouseButtonUp(0)
                        ? TouchPhase.Ended
                        : Input.GetMouseButton(0) ? TouchPhase.Moved : TouchPhase.Canceled;
                return touchPhase != TouchPhase.Canceled;
            }
            screenPosition = default;
            touchPhase = TouchPhase.Canceled;
            return false;
        }

        private void SaveTouch(Vector2 position)
        {
            _touchStartTime = Time.time;
            _inputStartPosition = position;
        }

        private void CheckTouch(Vector2 position)
        {
            var duration = Time.time - _touchStartTime;
            var moved = (position - _inputStartPosition).sqrMagnitude;
            if (duration <= _maxTapDuration && moved <= _maxSqrTapMovement)
            {
                _onScreenTapEnd.OnNext(_inputStartPosition);
            }
        }
    }
}

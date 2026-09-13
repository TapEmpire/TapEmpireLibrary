using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class InputCoreSystem : Initializable, IInputCoreSystem, ITickable
    {
        // Every touch the platform reports is read, however few are admitted: the index a touch
        // arrives under is not stable between frames, so reading only the admitted count could miss
        // a finger that is already tracked and drop it while it is still down.
        private const int MaxReportedTouches = 10;
        private const int DefaultFinger = 0;

        [field: SerializeField] public InputMode InputMode { get; private set; } = InputMode.Drag;

        [field: SerializeField] public int MaxTouches { get; set; } = 1;

        [SerializeField] private bool _blockInputOverUI = true;

        public ReactiveProperty<bool> BlockModeProperty { get; private set; }

        public Observable<Vector2> OnInputStart => _onInputStart;
        public Observable<Vector2> OnInputEnd => _onInputEnd;
        public Observable<Vector2> OnInputTapEnd => _onInputTapEnd;

        public Observable<TouchInput> OnTouchStart => _onTouchStart;
        public Observable<TouchInput> OnTouchEnd => _onTouchEnd;
        public Observable<TouchInput> OnTouchTapEnd => _onTouchTapEnd;

        public bool IsInputStart { get; private set; }
        public bool IsInputEnd { get; private set; }
        public bool IsInputHold { get; private set; }

        public IReadOnlyList<int> Fingers => _fingers;

        public bool IsSimulated { get; set; } = false;
        public Vector2 SimulatedPosition { get; set; } = Vector2.zero;

        private readonly Subject<Vector2> _onInputStart = new();
        private readonly Subject<Vector2> _onInputEnd = new();
        private readonly Subject<Vector2> _onInputTapEnd = new();

        private readonly Subject<TouchInput> _onTouchStart = new();
        private readonly Subject<TouchInput> _onTouchEnd = new();
        private readonly Subject<TouchInput> _onTouchTapEnd = new();

        private float _maxTapDuration = 0.2f;
        private float _maxSqrTapMovement = 100.0f;

        private readonly Dictionary<int, TouchState> _touches = new();
        private readonly List<int> _fingers = new();
        private readonly TouchSample[] _samples = new TouchSample[MaxReportedTouches];

        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _raycastResults = new();

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

        public Vector2 TouchPosition(int finger) => _touches.TryGetValue(finger).Position;
        public bool IsTouchStart(int finger) => _touches.TryGetValue(finger).IsStart;
        public bool IsTouchEnd(int finger) => _touches.TryGetValue(finger).IsEnd;
        public bool IsTouchHold(int finger) => _touches.TryGetValue(finger).IsHold;

        public void StartInput(Vector2 position, bool withPress = true)
        {
            if (IsSimulated)
            {
                SimulatedPosition = position;

                if (withPress)
                {
                    _onInputStart.OnNext(position);
                    _onTouchStart.OnNext(new TouchInput(DefaultFinger, position));
                }
            }
        }

        public void EndInput(Vector2 position)
        {
            if (IsSimulated)
            {
                _onInputEnd.OnNext(position);
                _onTouchEnd.OnNext(new TouchInput(DefaultFinger, position));
            }
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            BlockModeProperty = new ReactiveProperty<bool>(false);
            return base.OnInitializeAsync(cancellationToken);
        }

        public void Tick()
        {
            ResetInputFlags();

            if (BlockModeProperty.Value || IsSimulated)
            {
                _touches.Clear();
                _fingers.Clear();
                return;
            }

            var count = ReadTouchSamples();

            for (var i = 0; i < count; i++)
            {
                ApplyTouch(_samples[i]);
            }

            DropReleasedTouches();
        }

        // The mouse is the finger a desktop has, so it enters tracking the way a touch does.
        private int ReadTouchSamples()
        {
            var count = 0;

            if (Input.touchSupported)
            {
                var touchCount = Mathf.Min(Input.touchCount, MaxReportedTouches);

                for (var i = 0; i < touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    _samples[count++] = new TouchSample(touch.fingerId, touch.position, touch.phase);
                }
            }
            else
            {
                var phase = Input.GetMouseButtonDown(0)
                    ? TouchPhase.Began
                    : Input.GetMouseButtonUp(0)
                        ? TouchPhase.Ended
                        : Input.GetMouseButton(0) ? TouchPhase.Moved : TouchPhase.Canceled;

                if (phase != TouchPhase.Canceled)
                {
                    _samples[count++] = new TouchSample(DefaultFinger, Input.mousePosition, phase);
                }
            }

            return count;
        }

        private void ApplyTouch(TouchSample sample)
        {
            if (!_touches.TryGetValue(sample.Finger, out var state))
            {
                // A finger that begins with the slots full is never admitted, so it stays unknown
                // for the rest of its life rather than joining halfway through.
                if (sample.Phase != TouchPhase.Began || _touches.Count >= MaxTouches) return;

                state = new TouchState
                {
                    UIOwnsInput = _blockInputOverUI && IsPointerOverUI(sample.Position),
                };

                _fingers.Add(sample.Finger);
            }

            var began = sample.Phase == TouchPhase.Began;
            var held = sample.Phase is TouchPhase.Moved or TouchPhase.Stationary;
            var ended = sample.Phase is TouchPhase.Ended or TouchPhase.Canceled;

            if (began)
            {
                state.StartTime = Time.time;
                state.StartPosition = sample.Position;
            }

            state.Position = sample.Position;
            state.IsTracked = !ended;
            state.IsStart = began && !state.UIOwnsInput;
            state.IsHold = held && !state.UIOwnsInput;
            state.IsEnd = ended && !state.UIOwnsInput;

            IsInputStart |= state.IsStart;
            IsInputHold |= state.IsHold;
            IsInputEnd |= state.IsEnd;

            _touches[sample.Finger] = state;

            if (state.IsStart)
            {
                Publish(_onTouchStart, _onInputStart, sample.Finger, sample.Position);
            }

            if (!state.IsEnd) return;

            Publish(_onTouchEnd, _onInputEnd, sample.Finger, sample.Position);

            if (InputMode.HasFlag(InputMode.Tap))
            {
                CheckTap(sample.Finger, state, sample.Position);
            }
        }

        // The single-touch stream carries the primary finger alone, so one touch reads on both.
        private void Publish(Subject<TouchInput> perFinger, Subject<Vector2> primary, int finger, Vector2 position)
        {
            perFinger.OnNext(new TouchInput(finger, position));

            if (_fingers[0] == finger)
            {
                primary.OnNext(position);
            }
        }

        // A finger goes when it lifts, and equally when it stops being reported at all, or one the
        // platform loses would hold its slot for the rest of the session.
        private void DropReleasedTouches()
        {
            for (var i = _fingers.Count - 1; i >= 0; i--)
            {
                var finger = _fingers[i];
                var state = _touches[finger];

                if (state.IsTracked)
                {
                    state.IsTracked = false;
                    _touches[finger] = state;
                    continue;
                }

                _touches.Remove(finger);
                _fingers.RemoveAt(i);
            }
        }

        private void ResetInputFlags()
        {
            IsInputStart = false;
            IsInputEnd = false;
            IsInputHold = false;
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            _pointerEventData ??= new PointerEventData(eventSystem);
            _pointerEventData.position = screenPosition;

            _raycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _raycastResults);

            return _raycastResults.Count > 0;
        }

        private void CheckTap(int finger, TouchState state, Vector2 position)
        {
            var duration = Time.time - state.StartTime;
            var moved = (position - state.StartPosition).sqrMagnitude;

            if (duration <= _maxTapDuration && moved <= _maxSqrTapMovement)
            {
                Publish(_onTouchTapEnd, _onInputTapEnd, finger, state.StartPosition);
            }
        }
    }
}

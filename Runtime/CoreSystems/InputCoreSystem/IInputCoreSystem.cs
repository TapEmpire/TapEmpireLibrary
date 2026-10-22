using System.Collections.Generic;
using R3;
using UnityEngine;

namespace TapEmpire.CoreSystems
{
    public interface IInputCoreSystem : ICoreSystem
    {
        InputMode InputMode { get; }
        ReactiveProperty<bool> BlockModeProperty { get; }

        Observable<Vector2> OnInputStart { get; }
        Observable<Vector2> OnInputEnd { get; }
        Observable<Vector2> OnInputTapEnd { get; }

        Vector2 InputPosition { get; }
        bool IsInputStart { get; }
        bool IsInputEnd { get; }
        bool IsInputHold { get; }

        // Multitouch

        int MaxTouches { get; set; }
        IReadOnlyList<int> Fingers { get; }

        Observable<TouchInput> OnTouchStart { get; }
        Observable<TouchInput> OnTouchEnd { get; }
        Observable<TouchInput> OnTouchTapEnd { get; }

        Vector2 TouchPosition(int finger);
        bool IsTouchStart(int finger);
        bool IsTouchEnd(int finger);
        bool IsTouchHold(int finger);

        // Simulation

        bool IsSimulated { get; set; }
        Vector2 SimulatedPosition { get; set; }
        void StartInput(Vector2 position, bool withPress = true);
        void EndInput(Vector2 position);
    }
}

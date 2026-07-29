using R3;
using UnityEngine;

namespace TapEmpire.CoreSystems
{
    public interface IInputCoreSystem : ICoreSystem
    {
        ReactiveProperty<InputMode> InputModeProperty { get; }

        ReactiveProperty<bool> BlockModeProperty { get; }

        Observable<Vector2> OnScreenInputStart { get; }

        Observable<Vector2> OnScreenInputEnd { get; }

        Observable<Vector2> OnScreenTapEnd { get; }

        Vector2 InputPosition { get; }

        bool IsInputStart { get; }

        bool IsInputEnd { get; }

        bool IsInputHold { get; }

        // Simulation

        bool IsSimulated { get; set; }
        Vector2 SimulatedPosition { get; set; }
        void StartInput(Vector2 position, bool withPress = true);
        void EndInput(Vector2 position);
    }
}

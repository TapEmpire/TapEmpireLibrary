using UnityEngine;

namespace TapEmpire.CoreSystems
{
    public readonly struct TouchInput
    {
        public readonly int Finger;
        public readonly Vector2 Position;

        public TouchInput(int finger, Vector2 position)
        {
            Finger = finger;
            Position = position;
        }
    }
}

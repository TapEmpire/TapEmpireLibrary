using UnityEngine;

namespace TapEmpire.CoreSystems
{
    internal struct TouchState
    {
        public Vector2 Position;
        public Vector2 StartPosition;
        public float StartTime;
        public bool UIOwnsInput;
        public bool IsStart;
        public bool IsEnd;
        public bool IsHold;
        public bool IsTracked;
    }

    internal readonly struct TouchSample
    {
        public readonly int Finger;
        public readonly Vector2 Position;
        public readonly TouchPhase Phase;

        public TouchSample(int finger, Vector2 position, TouchPhase phase)
        {
            Finger = finger;
            Position = position;
            Phase = phase;
        }
    }
}

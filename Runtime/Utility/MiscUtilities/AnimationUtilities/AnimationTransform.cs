using UnityEngine;

namespace TapEmpire.Utility
{
    public enum PivotPosition
    {
        Default,
        Center,
    }

    [System.Serializable]
    public class AnimationTransform
    {
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.one;
        public PivotPosition PivotPosition = PivotPosition.Default; // Replacement for "Vector3 Position"

        public bool CanScatter = false; // The rotation might be shifted in various directions.

        public static readonly AnimationTransform Default = new AnimationTransform();

        public AnimationTransform Reflect()
        {
            return new AnimationTransform() { Rotation = -this.Rotation, Scale = this.Scale };
        }
    }
}

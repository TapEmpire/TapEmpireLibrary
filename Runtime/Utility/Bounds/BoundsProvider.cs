using UnityEngine;

namespace TapEmpire.Utility
{
    public abstract class BoundsProvider : MonoBehaviour, IBoundsProvider
    {
        public abstract Bounds Bounds { get; }
    }
}

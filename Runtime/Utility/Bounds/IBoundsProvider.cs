using UnityEngine;

namespace TapEmpire.Utility
{
    public interface IBoundsProvider
    {
        Bounds Bounds { get; }
    }
}

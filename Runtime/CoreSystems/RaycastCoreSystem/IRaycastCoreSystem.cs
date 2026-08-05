using System;
using UnityEngine;

namespace TapEmpire.CoreSystems
{
    public interface IRaycastCoreSystem : ICoreSystem
    {
        Vector2 InputWorldPoint { get; }

        RaycastHit2D RaycastHit2D { get; }

        RaycastHit2D RaycastHit2DLayered(LayerMask layerMask);

        Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB);

        ArraySegment<Collider2D> Overlap(Collider2D target);

        ArraySegment<Collider2D> OverlapTouching(Collider2D target);
    }
}

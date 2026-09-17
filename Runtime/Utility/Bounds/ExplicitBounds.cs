using UnityEngine;

namespace TapEmpire.Utility
{
    public class ExplicitBounds : BoundsProvider
    {
        [SerializeField] private Vector2 _size;
        [SerializeField] private Vector2 _center;

        public override Bounds Bounds => new(transform.position + (Vector3)_center, _size);

        public void SetBounds(Vector2 size, Vector2 center)
        {
            _size = size;
            _center = center;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    [AddComponentMenu("UI/Effects/UI Gradient")]
    [RequireComponent(typeof(Graphic))]
    public class UIGradient : BaseMeshEffect
    {
        public enum Direction
        {
            Vertical,
            Horizontal
        }

        [SerializeField] private Color topColor = Color.white;
        [SerializeField] private Color bottomColor = Color.black;
        [SerializeField] private Direction gradientDirection = Direction.Vertical;

        public Color TopColor { get => topColor; set { topColor = value; graphic.SetVerticesDirty(); } }
        public Color BottomColor { get => bottomColor; set { bottomColor = value; graphic.SetVerticesDirty(); } }
        public Direction GradientDirection { get => gradientDirection; set { gradientDirection = value; graphic.SetVerticesDirty(); } }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
                return;

            UIVertex vertex = new UIVertex();

            float top = float.MinValue;
            float bottom = float.MaxValue;
            float left = float.MaxValue;
            float right = float.MinValue;

            // First pass: find bounds
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                Vector3 pos = vertex.position;

                if (gradientDirection == Direction.Vertical)
                {
                    if (pos.y > top) top = pos.y;
                    if (pos.y < bottom) bottom = pos.y;
                }
                else
                {
                    if (pos.x < left) left = pos.x;
                    if (pos.x > right) right = pos.x;
                }
            }

            float height = top - bottom;
            float width = right - left;

            // Second pass: assign color
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                Vector3 pos = vertex.position;

                float t = gradientDirection == Direction.Vertical
                    ? (pos.y - bottom) / height
                    : (pos.x - left) / width;

                vertex.color = Color.Lerp(bottomColor, topColor, t);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}

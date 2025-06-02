using UnityEngine;

namespace TapEmpire.Utility
{
    public static class ParabolisticUtility
    {
        public struct ParabolaCoefficients
        {
            public float a, b, c;
        }

        public static ParabolaCoefficients GetParabola(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float x0 = p0.x, y0 = p0.y;
            float x1 = p1.x, y1 = p1.y;
            float x2 = p2.x, y2 = p2.y;

            float denominator = (x0 - x1) * (x0 - x2) * (x1 - x2);

            float a = (x2 * (y1 - y0) + x1 * (y0 - y2) + x0 * (y2 - y1)) / denominator;
            float b = (x2 * x2 * (y0 - y1) + x1 * x1 * (y2 - y0) + x0 * x0 * (y1 - y2)) / denominator;
            float c = (x1 * x2 * (x1 - x2) * y0 + x2 * x0 * (x2 - x0) * y1 + x0 * x1 * (x0 - x1) * y2) / denominator;

            return new ParabolaCoefficients { a = a, b = b, c = c };
        }

        public static float CalculateParabolisticPoint(float t, ParabolaCoefficients coefs)
            => Mathf.Pow(t, 2) * coefs.a + t * coefs.b + coefs.c;

        public static Vector3 CalculateSimpleParabolisticPoint(Vector3 start, Vector3 end, float height, float t)
        {
            Vector3 linear = Vector3.Lerp(start, end, t);

            float parabola = 4 * height * t * (1 - t);

            linear.y += parabola;

            return linear;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class MathUtility
    {
        private static readonly float TURNOVER_INVERSE = 1.0f / 180.0f;

        public static int LoopClamp(int value, int max)
        {
            return value < max ? value : 0;
        }

        public static float SmoothStep(float min, float max, float value)
        {
            var t = Mathf.InverseLerp(min, max, value);
            return t * t * (3f - 2f * t);
        }

        public static int LoopValue(int value, int max)
        {
            return (value + 1 >= max) ? 0 : value + 1;
        }

        public static int LoopValue(int value, int min, int max, int addend)
        {
            return min + ((value - min + addend) % (max - min));
        }

        public static int LoopValueBack(int value, int max)
        {
            return (value > 0) ? value - 1 : max - 1;
        }

        public static int AddCeiled(int value, int max)
        {
            return value < max ? value + 1 : max;
        }

        public static int AddCeiled(int value, int add, int max)
        {
            return Mathf.Min(value + add, max);
        }

        public static T Max<T>(T enumValue1, T enumValue2) where T : Enum
        {
            return Comparer<T>.Default.Compare(enumValue1, enumValue2) >= 0 ? enumValue1 : enumValue2;
            // return (T)(object)Mathf.Max(enumValue1.ToInt(), enumValue2.ToInt());
        }

        public static float RotationDistanceY(Quaternion from, Quaternion to)
        {
            return Mathf.Abs(Mathf.DeltaAngle(from.eulerAngles.y, to.eulerAngles.y));
        }

        public static float GetRotationTime(Vector3 from, Vector3 to, float turnAroundTime)
        {
            return (1 - Vector3.Dot(from, to)) * 0.5f * turnAroundTime;
        }

        public static float GetRotationTime(Quaternion from, Quaternion to, float turnAroundTime)
        {
            return RotationDistanceY(from, to) * turnAroundTime * TURNOVER_INVERSE;
        }

        public static float MaxRotationDistance(Vector3 from, Vector3 to)
        {
            return (to - from).MaxAbsValue();
        }

        public static int MultiplyValues(Vector3Int v)
        {
            return v.x * v.y * v.z;
        }

        public static Vector3 zShift(Vector3 vector, float z, bool shouldAdjust = false)
        {
            vector.z = shouldAdjust ? vector.z + z : z;
            return vector;
        }

        public static Vector3 yShift(Vector3 vector, float y, bool shouldAdjust = false)
        {
            vector.y = shouldAdjust ? vector.y + y : y;
            return vector;
        }

        public static Vector3 xShift(Vector3 vector, float x, bool shouldAdjust = false)
        {
            vector.x = shouldAdjust ? vector.x + x : x;
            return vector;
        }

        public static int ConvertToNumber(List<bool> list)
        {
            int number = 0;

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i])
                {
                    number |= 1 << i;
                }
            }

            return number;
        }

        public static int CeiledMod(int n, int div)
        {
            return (n + div - 1) / div;
        }

        public static bool IsPointInsideBox(Vector2 center, Vector2 target, Vector2 extents)
        {
            return Mathf.Abs(center.x - target.x) <= extents.x &&
                   Mathf.Abs(center.y - target.y) <= extents.y;
        }
    }
}
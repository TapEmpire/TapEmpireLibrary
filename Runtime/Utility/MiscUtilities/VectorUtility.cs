using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class VectorUtility
    {
        public static bool CheckIfEqualByDistance(Vector3 vectorA, Vector3 vectorB, float epsilon = 0.001f)
        {
            return Vector3.Distance(vectorA, vectorB) < epsilon;
        }
        
        public static bool TryGetNearestToPointAsVector2(Dictionary<string, Vector3> positionsDict, Vector3 point, out string nearestPositionKey, float minDistance = 0, float maxDistance = 1f)
        {
            return TryGetNearestToPoint(
                positionsDict.ToDictionary(keyValue => keyValue.Key, keyValue => (Vector2)keyValue.Value), point,
                out nearestPositionKey, minDistance, maxDistance);
        }
        
        public static bool TryGetNearestToPoint(Dictionary<string, Vector2> positionsDict, Vector2 point, out string nearestPositionKey, float minDistance = 0, float maxDistance = 1f)
        {
            var nearestDistance = float.MaxValue;
            nearestPositionKey = default;
            foreach (var (key, position) in positionsDict)
            {
                var distance = Vector2.Distance(point, position);
                if (distance <= maxDistance && distance >= minDistance && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPositionKey = key;
                }
            }
            return nearestPositionKey != default;
        }
    }
}
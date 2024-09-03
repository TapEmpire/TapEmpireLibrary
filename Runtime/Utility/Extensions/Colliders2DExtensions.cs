using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class Colliders2DExtensions
    {
        // юнитевские OverlapPoint и OverlapCollider работают криво
        public static bool CheckColliderContainsCollider(this Collider2D self, Collider2D targetCollider)
        {
            if (self is PolygonCollider2D polygonCollider2D)
            {
                return polygonCollider2D.CheckPolygonContainsCollider(targetCollider);
            }
            return self.CheckColliderContainsPoint(targetCollider.transform.position);
        }
        
        private static bool CheckPolygonContainsCollider(this PolygonCollider2D self, Collider2D targetCollider)
        {
            if (targetCollider is BoxCollider2D boxCollider)
            {
                return self.CheckPolygonContainsBoxCollider(boxCollider);
            }
            if (targetCollider is CircleCollider2D circleCollider)
            {
                return self.CheckPolygonContainsCircleCollider(circleCollider);
            }
            if (targetCollider is PolygonCollider2D targetPolygon)
            {
                return self.CheckPolygonContainsPolygonCollider(targetPolygon);
            }
            return false;
        }

        private static bool CheckPolygonContainsPolygonCollider(this PolygonCollider2D self, PolygonCollider2D targetPolygon)
        {
            foreach (Vector2 point in targetPolygon.points)
            {
                Vector2 worldPoint = targetPolygon.transform.TransformPoint(point);
                if (!self.CheckPolygonContainsPoint(worldPoint))
                {
                    return false;
                }
            }
            return true;
        }
        
        // юнитевские OverlapPoint и OverlapCollider работают криво
        
        public static bool CheckColliderContainsPoint(this Collider2D self, Vector2 point)
        {
            if (self is PolygonCollider2D polygonCollider2D)
            {
                return polygonCollider2D.CheckPolygonContainsPoint(point);
            }
            if (self is BoxCollider2D boxCollider)
            {
                var localPoint = (Vector2)boxCollider.transform.InverseTransformPoint(point) - boxCollider.offset;
                return Mathf.Abs(localPoint.x) <= boxCollider.size.x * 0.5f && Mathf.Abs(localPoint.y) <= boxCollider.size.y * 0.5f;
            }
            if (self is CircleCollider2D circleCollider)
            {
                var distance = Vector2.Distance(circleCollider.transform.position + (Vector3)circleCollider.offset, point);
                return distance <= circleCollider.radius;
            }
    
            Debug.LogError("CheckColliderContainsPoint not implemented for this collider type.");
            return false;
        }

        public static bool CheckPolygonContainsPoint(this PolygonCollider2D self, Vector2 point)
        {
            var polygonPoints = self.points;
            var polygonTransform = self.transform;
            var intersectCount = 0;
            for (var i = 0; i < polygonPoints.Length; i++)
            {
                var a = polygonTransform.TransformPoint(polygonPoints[i]);
                var b = polygonTransform.TransformPoint(polygonPoints[(i + 1) % polygonPoints.Length]);

                if ((a.y > point.y) != (b.y > point.y))
                {
                    var intersectX = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                    if (point.x < intersectX)
                    {
                        intersectCount++;
                    }
                }
            }
            return (intersectCount % 2) == 1;
        }

        private static bool CheckPolygonContainsBoxCollider(this PolygonCollider2D self, BoxCollider2D box)
        {
            var corners = new Vector2[4];

            float top = box.offset.y + (box.size.y / 2f);
            float btm = box.offset.y - (box.size.y / 2f);
            float left = box.offset.x - (box.size.x / 2f);
            float right = box.offset.x + (box.size.x / 2f);

            corners[0] = box.transform.TransformPoint(new Vector2(left, top));
            corners[1] = box.transform.TransformPoint(new Vector2(right, top));
            corners[2] = box.transform.TransformPoint(new Vector2(right, btm));
            corners[3] = box.transform.TransformPoint(new Vector2(left, btm));

            foreach (var corner in corners)
            {
                if (!self.CheckPolygonContainsPoint(corner))
                {
                    return false;
                }
     
            }

            return true;
        }

        private static bool CheckPolygonContainsCircleCollider(this PolygonCollider2D self, CircleCollider2D circleCollider)
        {
            return self.CheckPolygonContainsCircle((Vector2)circleCollider.transform.position, circleCollider.radius,
                circleCollider.offset);
        }   

        #region Sprite renderer size polygon
        
        public static void FitPolygonToSpriteRenderer8Corners(this PolygonCollider2D polygonCollider, SpriteRenderer spriteRenderer, 
            float widthOffset = 0f, float heightOffset = 0f)
        {
            var sprite = spriteRenderer.sprite;
            var border = sprite.border;
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var size = spriteRenderer.size;
            size.x += widthOffset;
            size.y += heightOffset;
            var offsetX = Mathf.Min(border.x / pixelsPerUnit, size.x / 4);
            var offsetY = Mathf.Min(border.y / pixelsPerUnit, size.y / 4);
            var points = new Vector2[]
            {
                new Vector2(-size.x / 2 + offsetX, -size.y / 2),                    
                new Vector2(-size.x / 2, -size.y / 2 + offsetY), 
                new Vector2(-size.x / 2, size.y / 2 - offsetY), 
                new Vector2(-size.x / 2 + offsetX, size.y / 2),    
                new Vector2(size.x / 2 - offsetX, size.y / 2),   
                new Vector2(size.x / 2, size.y / 2 - offsetY),  
                new Vector2(size.x / 2, -size.y / 2 + offsetY),
                new Vector2(size.x / 2 - offsetX, -size.y / 2),
            };

            polygonCollider.points = points;
        }
        
        public static void FitPolygonToRectWithRoundedCorners(this PolygonCollider2D polygonCollider, SpriteRenderer spriteRenderer, int pointsPerCorner, Vector2 cornerSize)
        {
            var sprite = spriteRenderer.sprite;
            var size = sprite.bounds.size;
            size.x -= cornerSize.x * 2;
            size.y -= cornerSize.y * 2;

            var points = new List<Vector2>();

            // Calculate points for each corner
            // Note: Arcs are added in a clockwise manner to maintain correct point order
            // Top-Right corner
            AddArc(points, new Vector2(size.x / 2, size.y / 2), cornerSize, 270, pointsPerCorner);  // Start from the bottom of the arc
            // Bottom-Right corner
            AddArc(points, new Vector2(size.x / 2, -size.y / 2), cornerSize, 0, pointsPerCorner);   // Start from the right of the arc
            // Bottom-Left corner
            AddArc(points, new Vector2(-size.x / 2, -size.y / 2), cornerSize, 90, pointsPerCorner);  // Start from the top of the arc
            // Top-Left corner
            AddArc(points, new Vector2(-size.x / 2, size.y / 2), cornerSize, 180, pointsPerCorner); // Start from the left of the arc

            polygonCollider.points = points.ToArray();
        }

        private static void AddArc(List<Vector2> points, Vector2 center, Vector2 radius, float startAngle, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.Deg2Rad * (startAngle + i * (90.0f / segments));
                points.Add(new Vector2(center.x + Mathf.Cos(angle) * radius.x, center.y + Mathf.Sin(angle) * radius.y));
            }
        }

        #endregion

        #region Contains circle

        public static bool CheckColliderContainsCircle(this Collider2D collider, Vector2 center, float radius, Vector2 offset)
        {
            if (collider is PolygonCollider2D polygon)
                return polygon.CheckPolygonContainsCircle(center, radius, offset);
            else if (collider is CircleCollider2D circle)
                return circle.CheckCircleContainsCircle(center, radius, offset);
            else if (collider is BoxCollider2D box)
                return box.CheckBoxContainsCircle(center, radius, offset);
            throw new NotSupportedException("Collider type not supported for this operation");
        }
        
        public static bool CheckPolygonContainsCircle(this PolygonCollider2D self, Vector2 center, float radius, Vector2 offset)
        {
            int numPoints = 20; 
            float angleStep = 360f / numPoints;

            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                point += center + offset;

                if (!self.CheckPolygonContainsPoint(point))
                {
                    return false;
                }
                    
            }
            return true;
        }
        
        public static bool CheckCircleContainsCircle(this CircleCollider2D self, Vector2 center, float radius, Vector2 offset)
        {
            var selfCenter = (Vector2)self.transform.position + self.offset;
            var selfRadius = self.radius;
            var distance = Vector2.Distance(selfCenter, center + offset);
            return distance + radius <= selfRadius;
        }
        
        public static bool CheckBoxContainsCircle(this BoxCollider2D self, Vector2 center, float radius, Vector2 offset)
        {
            var position = (Vector2)self.transform.position;
            var size = self.size;
            Vector2 topLeft = position + self.offset + new Vector2(-size.x / 2, size.y / 2);
            Vector2 bottomRight = position + self.offset + new Vector2(size.x / 2, -size.y / 2);
    
            int numPoints = 20; 
            float angleStep = 360f / numPoints;

            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                point += center + offset;

                if (point.x < topLeft.x || point.x > bottomRight.x || point.y > topLeft.y || point.y < bottomRight.y)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
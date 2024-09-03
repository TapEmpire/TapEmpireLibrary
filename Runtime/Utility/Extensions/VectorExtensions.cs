using UnityEngine;

namespace TapEmpire.Utility
{
    public static class VectorExtensions
    {
        public static Vector3 Divide(this Vector3 self, Vector3 other)
        {
            return new Vector3(self.x / other.x, self.y / other.y, self.z / other.z);
        }
        
        public static bool IsUniform(this Vector3 self, float tolerance = 0.0001f)
        {
            return Mathf.Abs(self.x - self.y) < tolerance && Mathf.Abs(self.y - self.z) < tolerance;
        }
        
        
        public static Vector2 WithDeltaX(this Vector2 self, float delta)
        {
            self.x += delta;
            return self;
        }
        
        public static Vector2 WithDeltaY(this Vector2 self, float delta)
        {
            self.y += delta;
            return self;
        }
        
        public static Vector3 WithDeltaX(this Vector3 self, float delta)
        {
            self.x += delta;
            return self;
        }
        
        public static Vector3 WithDeltaFactor(this Vector3 self, float delta)
        {
            self.x += delta;
            self.y += delta;
            self.z += delta;
            return self;
        }

        public static Vector3 WithDeltaY(this Vector3 self, float delta)
        {
            self.y += delta;
            return self;
        }
        
        public static Vector3 WithY(this Vector3 self, float y)
        {
            self.y = y;
            return self;
        }
        
        public static Vector3 WithZ(this Vector3 self, float z)
        {
            self.z = z;
            return self;
        }
        
        public static bool IsCollinear(this Vector2 self, Vector2 other)
        {
            var z = self.x * other.y - self.y * other.x;
            return Mathf.Approximately(z, 0);
        }
    }
}
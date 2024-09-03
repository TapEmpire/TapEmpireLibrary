using UnityEngine;

namespace TapEmpire.Utility
{
    public static class QuaternionExtensions
    {
        public static Quaternion RotateZ(this Quaternion self, float zDegrees)
        {
            float zRadians = zDegrees * Mathf.Deg2Rad;
            Quaternion zRotation = new Quaternion(0, 0, Mathf.Sin(zRadians / 2), Mathf.Cos(zRadians / 2));
            return self * zRotation;
        }
    }
}
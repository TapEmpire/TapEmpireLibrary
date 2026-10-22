using UnityEngine;

namespace TapEmpire.Utility
{
    public class Gyro : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}

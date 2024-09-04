using UnityEngine;

namespace TapEmpire.Services
{
    public class MonoCallbacksService : MonoBehaviour
    {
        public System.Action<bool> OnApplicationFocusChange = null;

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusChange?.Invoke(hasFocus);
        }
    }
}

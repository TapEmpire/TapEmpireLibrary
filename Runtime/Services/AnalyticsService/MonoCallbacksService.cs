using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MonoCallbacksService : MonoBehaviour
    {
        public Subject<bool> OnApplicationFocusChanged = new();

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusChanged.OnNext(hasFocus);
        }
    }
}

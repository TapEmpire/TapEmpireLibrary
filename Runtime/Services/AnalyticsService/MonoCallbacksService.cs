using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEL.Services
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

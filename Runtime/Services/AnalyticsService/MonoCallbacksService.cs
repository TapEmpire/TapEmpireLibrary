using System;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    public class MonoCallbacksService : MonoBehaviour
    {
        public Subject<bool> OnApplicationFocusChanged = new();
        public Action<bool> OnApplicationFocusChangedAction;

        private void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusChanged.OnNext(hasFocus);
            OnApplicationFocusChangedAction?.Invoke(hasFocus);
        }
    }
}

using System;
using UnityEngine;

namespace TapEmpire.UI
{
    public abstract class ToggleUIView : MonoBehaviour
    {
        public abstract void Initialize(Action<bool> onSetState, bool initialState);

        public abstract void Release();
    }
}
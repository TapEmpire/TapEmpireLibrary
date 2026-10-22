using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class DebugToggleLine : DebugLine
    {
        [SerializeField] private Toggle _toggle;

        public Toggle Toggle => _toggle;

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }
    }
}

using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public class DebugDropdownLine : DebugLine
    {
        [SerializeField] private TMP_Dropdown _dropdown;

        public TMP_Dropdown Dropdown => _dropdown;
    }
}

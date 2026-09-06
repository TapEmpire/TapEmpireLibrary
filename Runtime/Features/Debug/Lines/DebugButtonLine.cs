using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class DebugButtonLine : DebugLine
    {
        [SerializeField] private Button _button;

        public Button Button => _button;
    }
}

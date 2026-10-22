using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public class DebugTextLine : DebugLine
    {
        [SerializeField] private TMP_Text _value;

        public string Value
        {
            get => _value.text;
            set => _value.text = value;
        }
    }
}

using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public abstract class DebugLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }
    }
}

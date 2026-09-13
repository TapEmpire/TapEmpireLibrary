using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public class DebugInputLine : DebugLine
    {
        [SerializeField] private TMP_InputField _input;

        public TMP_InputField Input => _input;

        public string Text
        {
            get => _input.text;
            set => _input.text = value;
        }

        public void SetValue<T>(T value) => _input.text = value.ToString();

        public float GetFloat(float fallback) => float.TryParse(_input.text, out var value) ? value : fallback;

        public int GetInt(int fallback) => int.TryParse(_input.text, out var value) ? value : fallback;
    }
}

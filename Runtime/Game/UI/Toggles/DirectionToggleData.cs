using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class DirectionToggleData : MonoBehaviour
    {
        public Transform Handle;
        public Transform PositionOn;
        public Transform PositionOff;

        public TMP_Text EnabledText;
        public TMP_Text DisabledText;

        public TextData EnabledTextData;
        public TextData DisabledTextData;

        public void UpdateTexts(bool isEnabled)
        {
            UpdateText(EnabledText, isEnabled);
            UpdateText(DisabledText, !isEnabled);
        }

        private void UpdateText(TMP_Text text, bool isEnabled)
        {
            var data = isEnabled ? EnabledTextData : DisabledTextData;
            text.fontSizeMax = data.Size;
            text.fontSharedMaterial = data.Material;
            text.color = data.Color;
        }
    }

    [Serializable]
    public class TextData
    {
        public Material Material;
        public float Size;
        public Color Color;
    }
}
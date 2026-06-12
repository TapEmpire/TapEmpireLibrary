using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class CustomToggleButtonData : MonoBehaviour
    {
        public Image Background;
        public Sprite EnabledSprite;
        public Sprite DisabledSprite;
        public List<GameObject> DisabledObjects;
        public List<GameObject> EnabledObjects;

        public TMP_Text Text;
        public Material EnabledMaterial;
        public Material DisabledMaterial;

        public LocalizeStringEvent LocalizeEvent = null;
        public LocalizedString EnabledText;
        public LocalizedString DisabledText;
    }
}
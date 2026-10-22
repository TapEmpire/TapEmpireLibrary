using System.Collections.Generic;
using Sirenix.OdinInspector;
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
        [ShowIf("@Background != null")] public Sprite EnabledSprite;
        [ShowIf("@Background != null")] public Sprite DisabledSprite;
        public List<GameObject> DisabledObjects;
        public List<GameObject> EnabledObjects;

        public TMP_Text Text;
        [ShowIf("@Text != null")] public Material EnabledMaterial;
        [ShowIf("@Text != null")] public Material DisabledMaterial;

        public LocalizeStringEvent LocalizeEvent = null;
        [ShowIf("@LocalizeEvent != null")] public LocalizedString EnabledText;
        [ShowIf("@LocalizeEvent != null")] public LocalizedString DisabledText;
    }
}
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class CustomButton : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _enabledSprite;
        [SerializeField] private Sprite _disabledSprite;

        [SerializeField] private Image _icon;
        [SerializeField][ShowIf("@_icon != null")] private Color _enabledIconColor;
        [SerializeField][ShowIf("@_icon != null")] private Color _disabledIconColor;

        [SerializeField] private TMP_Text _text;
        [SerializeField][ShowIf("@_text != null")] private Material _enabledMaterial;
        [SerializeField][ShowIf("@_text != null")] private Material _disabledMaterial;

        public void SetActive(bool isActive)
        {
            _background.sprite = isActive ? _enabledSprite : _disabledSprite;

            if (_icon != null)
            {
                _icon.color = isActive ? _enabledIconColor : _disabledIconColor;
            }

            if (_text != null)
            {
                _text.fontSharedMaterial = isActive ? _enabledMaterial : _disabledMaterial;
            }
        }
    }
}

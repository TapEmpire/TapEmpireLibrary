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
        [SerializeField][ShowIf("@_icon != null")] private Sprite _enabledIconSprite;
        [SerializeField][ShowIf("@_icon != null")] private Sprite _disabledIconSprite;
        [SerializeField][ShowIf("@_icon != null && _disabledIconSprite == null")] private Color _enabledIconColor;
        [SerializeField][ShowIf("@_icon != null && _disabledIconSprite == null")] private Color _disabledIconColor;

        [SerializeField] private TMP_Text _text;
        [SerializeField][ShowIf("@_text != null")] private TMP_Text _disabledText;
        [SerializeField][ShowIf("@_text != null && _disabledText == null")] private Material _enabledMaterial;
        [SerializeField][ShowIf("@_text != null && _disabledText == null")] private Material _disabledMaterial;

        public void SetActive(bool isActive)
        {
            _background.sprite = isActive ? _enabledSprite : _disabledSprite;

            if (_icon != null)
            {
                if (_disabledIconSprite != null)
                {
                    _icon.sprite = isActive ? _enabledIconSprite : _disabledIconSprite;
                }
                else
                {
                    _icon.color = isActive ? _enabledIconColor : _disabledIconColor;
                }
            }

            if (_text != null)
            {
                if (_disabledText != null)
                {
                    _text.gameObject.SetActive(isActive);
                    _disabledText.gameObject.SetActive(!isActive);
                }
                else
                {
                    _text.fontSharedMaterial = isActive ? _enabledMaterial : _disabledMaterial;
                }
            }
        }
    }
}

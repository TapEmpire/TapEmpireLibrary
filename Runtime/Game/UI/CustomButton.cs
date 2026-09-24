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

        [SerializeField] private GameObject _enabledText;
        [SerializeField][ShowIf("@_enabledText != null")] private GameObject _disabledText;

        [SerializeField][ShowIf("@_enabledText == null")] private TMP_Text _text;
        [SerializeField][ShowIf("@_enabledText == null && _text != null")] private Material _enabledMaterial;
        [SerializeField][ShowIf("@_enabledText == null && _text != null")] private Material _disabledMaterial;

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

            if (_enabledText != null)
            {
                _enabledText.SetActive(isActive);
                _disabledText.SetActive(!isActive);
            }
            else if (_text != null)
            {
                _text.fontSharedMaterial = isActive ? _enabledMaterial : _disabledMaterial;
            }
        }
    }
}

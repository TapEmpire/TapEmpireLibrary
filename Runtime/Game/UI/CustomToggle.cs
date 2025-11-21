using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class CustomToggle : Toggle
    {
        [SerializeField] private GameObject _handle;
        [SerializeField] private Transform _onPosition;
        [SerializeField] private Transform _offPosition;

        [SerializeField] private Color32 _onBackground = Color.green;
        [SerializeField] private Color32 _offBackground = Color.gray;

        [SerializeField] private Image _image = null;
        [SerializeField] private Sprite _offTexture = null;
        [SerializeField] private Sprite _onTexture = null;

        protected CustomToggle() : base()
        {
            onValueChanged.AddListener(OnValueChanged);
        }


        private void OnValueChanged(bool isOn)
        {
            var targetPosition = isOn ? _onPosition.localPosition : _offPosition.localPosition;
            _handle.transform.DOLocalMove(targetPosition, 0.1f);

            targetGraphic.color = isOn ? _onBackground : _offBackground;

            if (_onTexture != null && _offTexture != null)
            {
                _image.sprite = isOn ? _onTexture : _offTexture;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}
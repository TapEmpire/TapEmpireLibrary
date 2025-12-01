using R3;
using TapEmpire.Utility;
using TEL.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    [RequireComponent(typeof(CustomToggleButtonData))]
    public class CustomToggleButton : Toggle
    {
        [SerializeField][ReadOnly] private CustomToggleButtonData _data;
        private CompositeDisposable _disposables = new();

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.Subscribe(OnValueChanged).AddTo(_disposables);
        }

        private void OnValueChanged(bool isOn)
        {
            _data.Background.sprite = isOn ? _data.EnabledSprite : _data.DisabledSprite;
            _data.Text.fontSharedMaterial = isOn ? _data.EnabledMaterial : _data.DisabledMaterial;

            if (_data.LocalizeEvent != null)
            {
                _data.LocalizeEvent.StringReference = isOn ? _data.EnabledText : _data.DisabledText;
            }
        }

        protected override void OnDestroy()
        {
            _disposables.Dispose();
            base.OnDestroy();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _data = GetComponent<CustomToggleButtonData>();
        }
#endif
    }
}
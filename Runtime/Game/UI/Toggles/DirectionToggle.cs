using DG.Tweening;
using R3;
using TapEmpire.Utility;
using TEL.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    [RequireComponent(typeof(DirectionToggleData))]
    public class DirectionToggle : Toggle
    {
        [SerializeField][ReadOnly] protected DirectionToggleData _data;
        protected CompositeDisposable _disposables = new();

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.Subscribe(OnValueChanged).AddTo(_disposables);
        }

        protected virtual void OnValueChanged(bool isOn)
        {
            var targetPosition = isOn ? _data.PositionOn.localPosition : _data.PositionOff.localPosition;
            _data.Handle.transform.DOKill();
            _data.Handle.transform
                .DOLocalMove(targetPosition, 0.1f)
                .OnComplete(() => _data.UpdateTexts(isOn));
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
            _data = GetComponent<DirectionToggleData>();
        }
#endif
    }
}
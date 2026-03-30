using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using R3;
using TapEmpire.Services.LiveOps;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsIcon : MonoBehaviour, IDisposable
    {
        [SerializeField] protected Button _button;
        [SerializeField] protected TMP_Text _timer;
        [SerializeField] protected TMP_Text _counter;
        [SerializeField] protected GameObject _indicator;
        [SerializeField] protected Image _progress;

        protected ILiveOps _liveOps = null;
        protected CompositeDisposable _disposables = new();

        public virtual void Initialize(ILiveOps liveOps)
        {
            _liveOps = liveOps;
            _button.onClick.Subscribe(OnButtonPressed).AddTo(_disposables);
        }

        public virtual void Dispose()
        {
            _disposables.Dispose();
        }

        public virtual void Animate(int addend, bool debug = false)
        {
        }

        protected virtual void OnDestroy() => Dispose();

        protected virtual void OnButtonPressed()
        {
            _liveOps.OpenView();
        }

        protected T LiveOps<T>() where T : ILiveOps => (T)_liveOps;

        protected void AddProgressAnimation(Sequence sequence, float fillAmount, int amountToClaim, bool isAmountIncreased)
        {
            var fill = isAmountIncreased ? 1.0f : fillAmount;
            _progress.DOFillAmount(fill, 0.3f).AppendTo(sequence);

            if (isAmountIncreased)
            {
                var duration = sequence.Duration();
                _indicator.SetActive(true);
                _indicator.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0).AppendTo(sequence);
                sequence.InsertCallback(duration + 0.05f, () =>
                {
                    _counter.text = amountToClaim.ToString();
                    _progress.fillAmount = 0.0f;
                });

                if (fillAmount < 1.0f - float.Epsilon)
                {
                    _progress.DOFillAmount(fillAmount, 0.3f).AppendTo(sequence);
                }
            }
        }
    }
}

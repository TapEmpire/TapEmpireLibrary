using System;
using System.Collections;
using System.Collections.Generic;
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
            // _progress.fillAmount = ;
        }

        public virtual void Dispose()
        {
            _disposables.Dispose();
        }

        public virtual void Animate(int addend)
        {
        }

        protected virtual void OnDestroy() => Dispose();

        protected virtual void OnButtonPressed()
        {
            _liveOps.OpenView();
        }

        protected T LiveOps<T>() where T : ILiveOps => (T)_liveOps;
    }
}

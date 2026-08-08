using System;
using R3;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public abstract class DebugComponent : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private Button _applyButton;

        protected readonly CompositeDisposable _disposables = new();

        public string Header
        {
            get => _header.text;
            set => _header.text = value;
        }

        public virtual IDisposable Initialize()
        {
            if (_applyButton != null)
            {
                _applyButton.onClick.Subscribe(Apply).AddTo(_disposables);
            }

            Read();
            return this;
        }

        public virtual void Dispose() => _disposables.Dispose();

        public virtual void Read() { }
        public virtual void Apply() { }
    }
}

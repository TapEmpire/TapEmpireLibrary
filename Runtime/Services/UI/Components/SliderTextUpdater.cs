using System;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class SliderTextUpdater : MonoBehaviour
    {
        [SerializeField] Slider _slider;

        private TMP_Text _text;
        private IDisposable _disposable;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _disposable = _slider.onValueChanged.Subscribe(value => _text.text = value.ToString());
            _text.text = _slider.value.ToString();
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }
    }
}
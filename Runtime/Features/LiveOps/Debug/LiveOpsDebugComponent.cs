using System;
using System.Globalization;
using System.Linq;
using R3;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsDebugComponent : MonoBehaviour
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private Button _apply;
        [SerializeField] private TMP_Dropdown _state;
        [SerializeField] private TMP_InputField _id;
        [SerializeField] private TMP_InputField _inner;
        [SerializeField] private TMP_InputField _value;
        [SerializeField] private TMP_InputField _addend;
        [SerializeField] private TMP_InputField _endTime;
        [SerializeField] private LiveOpsTimeUnit _timeUnit = LiveOpsTimeUnit.Minutes;

        private ILiveOps _liveOps;
        private ILiveOpsDebugExtension _extension;
        private CompositeDisposable _disposables = new();

        public string EndTimeText => _endTime.text;

        public bool TryParseEndTime(out DateTime endTime)
        {
            return DateTime.TryParse(_endTime.text, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out endTime);
        }

        public void Initialize(ILiveOps liveOps)
        {
            _liveOps = liveOps;
            _extension = GetComponent<ILiveOpsDebugExtension>();

            _header.text = _liveOps.Name;
            _apply.GetComponentInChildren<TMP_Text>().text = $"{_liveOps.Name} Apply";

            _state.ClearOptions();
            _state.AddOptions(Enum.GetNames(typeof(State)).ToList());

            _extension?.Initialize(_liveOps);

            _apply.onClick.Subscribe(Apply).AddTo(_disposables);
        }

        public void Dispose()
        {
            _extension?.Dispose();
            _disposables.Dispose();
        }

        public void Read()
        {
            var runtime = _liveOps.Runtime;
            _state.value = (int)runtime.State;
            _state.RefreshShownValue();
            _id.text = runtime.Id ?? string.Empty;
            _inner.text = runtime.Inner.ToString();
            _value.text = runtime.Value.ToString();
            _addend.text = runtime.Addend.ToString();
            _endTime.text = FormatEndTime();

            _extension?.Read(_liveOps);
        }

        private string FormatEndTime()
        {
            var remaining = _liveOps.GetRemainingTime();
            var format = _timeUnit == LiveOpsTimeUnit.Days ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss";
            return remaining > TimeSpan.Zero && remaining.TotalDays < 365
                ? (DateTime.UtcNow + remaining).ToString(format, CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private void Apply()
        {
            var runtime = _liveOps.Runtime;
            runtime.State = (State)_state.value;
            runtime.Id = string.IsNullOrEmpty(_id.text) ? runtime.Id : _id.text;
            runtime.Inner = int.TryParse(_inner.text, out int inner) ? inner : runtime.Inner;
            runtime.Value = int.TryParse(_value.text, out int value) ? value : runtime.Value;
            runtime.Addend = int.TryParse(_addend.text, out int addend) ? addend : runtime.Addend;

            if (TryParseEndTime(out var endTime))
                _liveOps.SetEndTime(endTime);

            _extension?.Apply(_liveOps);

            _liveOps.Save();
        }
    }
}

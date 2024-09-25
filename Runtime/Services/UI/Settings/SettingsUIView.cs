using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class SettingsPopupView : UIView<SettingsUIViewModel>, IFadeAbleView
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        
        [SerializeField]
        private Button _backgroundButton;

        [SerializeField]
        private Button _closeButton;
        
        [Header("Version")]
        [SerializeField]
        private TextMeshProUGUI _versionText;

        [SerializeField]
        private string _versionFormat = "version {0}";

        [Header("Settings")]
        [SerializeField]
        private SerializableDictionary<SettingsToggleCode, ToggleUIView> _togglesDict;
        
        [SerializeField]
        private ToggleUIView _musicToggle;
        
        [SerializeField]
        private ToggleUIView _soundToggle;

        public CanvasGroup CanvasGroup => _canvasGroup;
        
        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _backgroundButton.onClick.AddListener(DerivedModel.OnClose.Invoke);
            _closeButton.onClick.AddListener(DerivedModel.OnClose.Invoke);

            foreach (var (code, view) in _togglesDict)
            {
                if (DerivedModel.TryGetToggleData(code, out var callback, out var startState))
                {
                    view.gameObject.SetActive(true);
                    view.Initialize(callback, startState);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
            _versionText.text = string.Format(_versionFormat, Application.version);
            
            return UniTask.CompletedTask;
        }
        
        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _backgroundButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            
            foreach (var (code, view) in _togglesDict)
            {
                if (DerivedModel.CheckToggleActive(code))
                {
                    view.Release();
                }
            }
            return UniTask.CompletedTask;
        }
    }
}
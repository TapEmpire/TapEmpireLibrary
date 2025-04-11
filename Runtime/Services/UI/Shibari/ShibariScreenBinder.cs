using UnityEngine;
using Zenject;

namespace TapEmpire.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ShibariScreenBinder : MonoBehaviour
    {
        [SerializeField]
        private string _shibariName = "";

        private IUIService _uiService;

        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
            AddToContext();
        }

        public void Initialize(IUIService uiService, string shibariName)
        {
            _uiService = uiService;
            SetShibariName(shibariName);
        }

        public void SetShibariName(string shibariName)
        {
            _shibariName = shibariName;
            AddToContext();
        }

        private void OnEnable()
        {
            AddToContext();
        }

        private void OnDisable()
        {
            _uiService?.RemoveFromContext(_shibariName);
        }

        private void AddToContext()
        {
            if (!string.IsNullOrEmpty(_shibariName))
            {
                _uiService?.AddToContext(_shibariName, GetComponent<RectTransform>());
            }
        }
    }
}
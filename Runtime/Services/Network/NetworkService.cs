using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TapEmpire.Settings;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class NetworkService : Initializable, INetworkService
    {
        [SerializeField]
        private bool _waitInInitialize = true;

        [SerializeField, ShowIf(nameof(_waitInInitialize))]
        private bool _waitInInitializeWithUI = true;
        
        [SerializeField]
        private NoInternetUIView _noInternetUIViewPrefab;

        public bool HasConnection
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return _systemService.CanPlayOffline || Application.internetReachability != NetworkReachability.NotReachable;
#endif
            }
        }

        private IUIService _uiService;
        private ISystemService _systemService;
        
        [Inject]
        private void Construct(IUIService uiService, ISystemService systemService)
        {
            _uiService = uiService;
            _systemService = systemService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            return _waitInInitialize
                ? WaitNetworkAsync(cancellationToken, _waitInInitializeWithUI)
                : UniTask.CompletedTask;
        }

        public async UniTask WaitNetworkAsync(CancellationToken cancellationToken, bool withUI)
        {
            if (HasConnection)
            {
                return;
            }
            if (withUI)
            {
                if (_noInternetUIViewPrefab == null)
                {
                    Debug.Log($"No NoInternetPopupUIView prefab in NetworkService");
                }
                else
                {
                    var popupModel = new NoInternetUIViewModel();
                    await _uiService.OpenViewAsync(_noInternetUIViewPrefab, popupModel, cancellationToken, asPopup: true);
                }
            }
            await UniTask.WaitUntil(() => HasConnection, cancellationToken: cancellationToken);
             if (withUI && _noInternetUIViewPrefab != null)
             {
                 await _uiService.TryCloseViewAsync<NoInternetUIViewModel>(cancellationToken);
             }
        }
    }
}
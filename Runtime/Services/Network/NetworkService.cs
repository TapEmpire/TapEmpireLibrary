using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
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

        public bool HasConnection => Application.internetReachability != NetworkReachability.NotReachable;

        private IUIService _uiService;
        
        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
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
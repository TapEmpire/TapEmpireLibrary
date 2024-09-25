using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.UI;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class NetworkService : Initializable, INetworkService
    {
        [SerializeField]
        private NoInternetPopupUIView _popupPrefab;

        public bool HasConnection => Application.internetReachability != NetworkReachability.NotReachable;

        private IUIService _uiService;
        
        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        public async UniTask WaitNetworkAsync(CancellationToken cancellationToken, bool withUI)
        {
            if (HasConnection)
            {
                return;
            }
            if (withUI)
            {
                var popupModel = new NoInternetPopupUIViewModel();
                await _uiService.OpenViewAsync(_popupPrefab, popupModel, cancellationToken);
            }
            await UniTask.WaitUntil(() => HasConnection, cancellationToken: cancellationToken);
            if (withUI)
            {
                await _uiService.TryCloseViewAsync<NoInternetPopupUIViewModel>(cancellationToken);
            }
        }
    }
}
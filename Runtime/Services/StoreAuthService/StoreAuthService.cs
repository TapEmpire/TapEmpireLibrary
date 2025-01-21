using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class StoreAuthService : Initializable, IStoreAuthService
    {
        [SerializeField] private StoreAuthSettings _storeAuthSettings;
        
        private IStoreAuthAdapter _storeAuthAdapter;

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _storeAuthAdapter = new AndroidStoreAuthAdapter();
            if (_storeAuthSettings.AutoLogin)
            {
                _storeAuthAdapter.Login();
            }
            return base.OnInitializeAsync(cancellationToken);
        }

        public void Login()
        {
            _storeAuthAdapter.Login();
        }
        
        public void Logout()
        {
            _storeAuthAdapter.Logout();
        }
    }

}
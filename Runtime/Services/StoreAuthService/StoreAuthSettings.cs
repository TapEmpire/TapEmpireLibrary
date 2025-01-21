using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/StoreAuthSettings", fileName = "StoreAuthSettings")]
    public class StoreAuthSettings : ScriptableObject
    {
        [SerializeField] private bool _autoLogin;
        public bool AutoLogin => _autoLogin;
    }
}
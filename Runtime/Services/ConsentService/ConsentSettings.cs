using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/ConsentSettings", fileName = "ConsentSettings")]
    public class ConsentSettings : ScriptableObject
    {
        public bool EnableUmp = true;
        public bool EnableAtt = true;
        public bool IsForFamily = false;
        public List<string> TestDeviceHashedIds = new();
    }
}

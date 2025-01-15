using System;

namespace TapEmpire.Build
{
    [Serializable]
    public class KeystoreJsonData
    {
        public string keystorePath;
        public string keystorePass;
        public string keyAlias;
        public string keyPass;
    }
}
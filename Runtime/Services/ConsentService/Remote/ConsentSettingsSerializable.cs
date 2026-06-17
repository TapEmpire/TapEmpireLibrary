using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    [Serializable]
    public class ConsentSettingsSerializable : RemoteSerializableBase<ConsentSettings, ConsentSettingsSerializable.ConsentRemoteModel>
    {
        public override string TokenName => "ConsentSettings";

        public class ConsentRemoteModel : IRemoteModel<ConsentSettings>
        {
            public bool? EnableUmp;
            public bool? EnableAtt;
            public bool? IsForFamily;
            public List<string> TestDeviceHashedIds;

            public void FromSettings(ConsentSettings settings)
            {
                EnableUmp = settings.EnableUmp;
                EnableAtt = settings.EnableAtt;
                IsForFamily = settings.IsForFamily;
                TestDeviceHashedIds = settings.TestDeviceHashedIds.ToList();
            }

            public void ToSettings(ConsentSettings settings)
            {
                settings.EnableUmp = EnableUmp ?? settings.EnableUmp;
                settings.EnableAtt = EnableAtt ?? settings.EnableAtt;
                settings.IsForFamily = IsForFamily ?? settings.IsForFamily;
                if (TestDeviceHashedIds != null)
                {
                    settings.TestDeviceHashedIds.Clear();
                    settings.TestDeviceHashedIds.AddRange(TestDeviceHashedIds);
                }
            }
        }
    }
}

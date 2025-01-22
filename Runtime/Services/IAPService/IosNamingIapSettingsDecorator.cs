using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class IosNamingIapSettingsDecorator : IIapSettingsDecorator
    {
        public List<PackIapSettings> Process(List<PackIapSettings> settings)
        {
#if UNITY_IOS
            for (var index = 0; index < settings.Count; index++)
            {
                var iapSettings = settings[index];
                var bundleName = Application.identifier;
                if (iapSettings.Key.Contains(bundleName))
                    continue;
                iapSettings.Key = $"{bundleName}.{iapSettings.Key}";
            }
#endif
            return settings;
        }
    }
}
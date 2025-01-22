using System.Collections.Generic;

namespace TapEmpire.Services
{
    interface IIapSettingsDecorator
    {
        List<PackIapSettings> Process(List<PackIapSettings> settings);
    }
}
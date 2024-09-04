using System.Collections.Generic;

namespace TapEmpire.Services
{
    public interface IAnalyticsService : IService
    {
        void LogEvent(string eventName, Dictionary<string,object> eventParams);
        void SetUserProperty(string propertyName, int value);
        void SetUserProperties(IDictionary<string, object> properties);
    }
}

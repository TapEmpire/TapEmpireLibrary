using System.Collections.Generic;

namespace TapEmpire.Services
{
    public enum AnalyticsType
    {
        Amplitude,
        GameAnalytics,
    }

    public interface IAnalyticsService : IService
    {
        void LogEvent(string eventName, Dictionary<string,object> eventParams);
        void SetUserProperty(string propertyName, int value);
        void SetUserProperty(string propertyName, string value);
        void SetUserProperties(IDictionary<string, object> properties);
        void FlushEvents();
    }
}

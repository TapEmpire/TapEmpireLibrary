using System.Collections.Generic;

namespace TapEmpire.Services
{
    public enum AnalyticsType
    {
        Amplitude,
        GameAnalytics,
    }

    public enum ProgressionState
    {
        Undefined = 0,
		Start = 1,
		Complete = 2,
		Fail = 3
    }

    public interface IAnalyticsService : IService
    {
        void LogEvent(string eventName, Dictionary<string,object> eventParams);
        void SetUserProperty(string propertyName, int value);
        void SetUserProperty(string propertyName, string value);
        void SetUserProperties(IDictionary<string, object> properties);
        void FlushEvents();
        
        void LogEvent(string eventName, int value);

        void LogProgressionEvent(ProgressionState state, string progression01, string progression02, string progression03);
    }
}

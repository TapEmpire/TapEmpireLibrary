using TapEmpire.Patterns.Strategy;

namespace TapEmpire.Services.Notifications
{
    public interface IPushCondition : ISubject
    {
    }

    public abstract class PushConditionHandler<T> : BaseHandler<T> where T : IPushCondition
    {
    }
}
using TapEmpire.Patterns.Strategy;

namespace TapEmpire.Services.LiveOps
{
    public interface ICondition : ISubject { }

    public abstract class ConditionHandler<T> : BaseHandler<T> where T : ICondition { }
}
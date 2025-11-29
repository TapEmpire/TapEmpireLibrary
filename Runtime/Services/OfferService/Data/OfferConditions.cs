using TapEmpire.Patterns.Strategy;

namespace TapEmpire.Services.Offer
{
    public interface ICondition : ISubject { }

    public abstract class ConditionHandler<T> : BaseHandler<T> where T : ICondition { }
}
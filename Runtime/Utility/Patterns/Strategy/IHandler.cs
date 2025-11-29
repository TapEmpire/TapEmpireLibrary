using System;
using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.Patterns.Strategy
{
    public interface IHandler
    {
        bool CanHandle(ISubject subject);
        bool Handle(ISubject subject);
        Type GetSubjectType();

        void Initialize(DiContainer diContainer);
    }

    public interface IHandler<T> : IHandler where T : ISubject
    {
        bool Handle(T product);
    }

    public abstract class BaseHandler<T> : IHandler<T> where T : ISubject
    {
        public bool CanHandle(ISubject subject) => subject is T;

        public bool Handle(ISubject subject)
        {
            if (subject is T concreteProduct)
                return Handle(concreteProduct);

            throw new ArgumentException("Invalid product type");
        }

        public abstract bool Handle(T subject);
        public abstract void Initialize(DiContainer diContainer);
        public virtual Type GetSubjectType() => typeof(T);
    }
}
using System;
using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.Services
{
    public interface IIapHandler<T> : IIapHandler where T : IIapProduct
    {
        UniTask Handle(T product, string placement);
    }

    public interface IIapHandler
    {
        bool CanHandle(IIapProduct product);
        UniTask Handle(IIapProduct product, string placement);
        Type GetProductType();

        void Initialize(DiContainer diContainer);
    }

    public abstract class BaseIapHandler<T> : IIapHandler<T> where T : IIapProduct
    {
        public bool CanHandle(IIapProduct product) => product is T;

        public UniTask Handle(IIapProduct product, string placement)
        {
            if (product is T concreteProduct)
                return Handle(concreteProduct, placement);

            throw new ArgumentException("Invalid product type");
        }

        public abstract UniTask Handle(T product, string placement);
        public abstract void Initialize(DiContainer diContainer);
        public virtual Type GetProductType() => typeof(T);
    }
}
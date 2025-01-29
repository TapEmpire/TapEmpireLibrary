using System;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IIapHandler<T> : IIapHandler where T : IIapProduct
    {
        UniTask Handle(T product);
    }
    
    public interface IIapHandler
    {
        bool CanHandle(IIapProduct product);
        UniTask Handle(IIapProduct product);
    }

    public abstract class BaseIapHandler<T> : IIapHandler<T> where T : IIapProduct
    {
        public bool CanHandle(IIapProduct product) => product is T;
    
        public UniTask Handle(IIapProduct product)
        {
            if (product is T concreteProduct)
                return Handle(concreteProduct);
        
            throw new ArgumentException("Invalid product type");
        }

        public abstract UniTask Handle(T product);
    }
}
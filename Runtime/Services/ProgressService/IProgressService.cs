using System;

namespace TapEmpire.Services
{
    public interface IProgressService : IService
    {
        ICachedReactiveDictionary<int> IntValuesDictionary { get; }
        
        ICachedReactiveDictionary<bool> BoolValuesDictionary { get; }
        
        ICachedReactiveDictionary<string> StringValuesDictionary { get; }
        
        void ClearProgress();
        
        event Action OnClearProgress;
        bool TryLoad<T>(string key, out T item);
        void Save<T>(string key, T item);
    }
}
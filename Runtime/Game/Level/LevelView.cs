using System;
using System.Linq;
using TapEmpire.Utility;
using UnityEngine;
using TapEmpire.CoreSystems;

namespace TapEmpire.Level
{
    public class LevelView : MonoBehaviour
    {
        public SerializableReferencesDictionary<string, ILevelReference> LevelData = new();

        [SerializeReference] private IExecutionModule[] _levelModules = Array.Empty<IExecutionModule>();

        public IExecutionModule[] LevelModules => _levelModules;

        [field: SerializeField] public Camera Camera { get; set; }

        public void AddLevelModule(IExecutionModule module)
        {
            _levelModules = _levelModules.Append(module).ToArray();
        }

        public T GetLevelModule<T>() where T : IExecutionModule
        {
            return _levelModules.OfType<T>().FirstOrDefault();
        }

        public bool TryGetLevelReference<T>(string key, out T value)
        {
            if (LevelData.TryGetValue(key, out var referenceValue))
            {
                value = (referenceValue as ILevelReference<T>).Reference;
                return true;
            }

            value = default;
            return false;
        }

        public T TryGetLevelReference<T>(string key)
        {
            return (LevelData.TryGetValue(key) as ILevelReference<T>).Reference;
        }

        public T GetLevelReference<T>(string key)
        {
            return TryGetLevelReference<T>(key, out var value) ? value : default;
        }
    }
}
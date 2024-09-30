using System;
using UnityEngine;

namespace TapEmpireLibrary.Game
{
    public class MonoBehaviourCallbacks : MonoBehaviour, IGameEventsContainer
    {
        public event Action OnApplicationQuitEvent;
        
        private void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }
    }
}
using System;

namespace TapEmpireLibrary.Game
{
    public interface IGameEventsContainer
    {
        event Action OnApplicationQuitEvent;
    }
}
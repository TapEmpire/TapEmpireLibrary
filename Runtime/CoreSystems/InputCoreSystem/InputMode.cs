using System;

namespace TapEmpire.CoreSystems
{
    [Flags]
    public enum InputMode
    {
        None = 0,
        Drag = 1 << 0,
        Tap = 1 << 1,
        Drawing = 1 << 2
    }
}

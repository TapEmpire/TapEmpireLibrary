using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace TapEmpire.CoreSystems
{
    public interface IRaycastUICoreSystem : ICoreSystem
    {
        IEnumerable<RaycastResult> RaycastHitUI { get; }

        IEnumerable<RaycastResult> RaycastHitTaggedUI(string tag);
    }
}

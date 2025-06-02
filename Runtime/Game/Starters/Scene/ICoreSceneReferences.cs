using UnityEngine;

namespace TapEmpire.Level
{
    public interface ICoreSceneReferences
    {
        Light Light { get; }

        Transform LevelParent { get; }
        GameObject BackgroundSprite { get; }

    }
}
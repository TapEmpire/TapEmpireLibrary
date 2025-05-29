using UnityEngine;

namespace TapEmpire.Level
{
    public class CoreSceneReferences : MonoBehaviour, ICoreSceneReferences
    {
        [field: SerializeField] public Light Light { get; private set; }
        [field: SerializeField] public GameObject BackgroundSprite { get; private set; }
        [field: SerializeField] public Transform LevelParent { get; private set; }
    }
}
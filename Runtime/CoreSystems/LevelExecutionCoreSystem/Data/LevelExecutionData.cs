using TapEmpire.Level;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TapEmpire.CoreSystems
{
    public class LevelExecutionData
    {
        public readonly LevelSettings LevelSettings;
        public readonly LevelView LevelView;
        public readonly int LevelIndex = -1;
        public readonly LevelStateData LevelStateData;

        public Camera Camera => LevelView.Camera;

        public LevelExecutionData(LevelSettings levelSettings, LevelView levelView, int levelIndex)
        {
            LevelSettings = levelSettings;
            LevelView = levelView;
            LevelIndex = levelIndex;
            LevelStateData = new LevelStateData();
        }

        public void Release()
        {
            if (LevelView != null)
            {
                LevelView.LevelModules.ForEach(module => module.Release());
                Addressables.ReleaseInstance(LevelView.gameObject);
                GameObject.Destroy(LevelView.gameObject);
            }
        }
    }
}
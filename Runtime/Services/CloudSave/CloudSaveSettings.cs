using UnityEngine;

namespace TapEmpire.Services
{
    public enum CloudSaveTrackedValueType
    {
        Unknown = 0,
        Int = 1,
        Bool = 2,
        String = 3,
    }

    [System.Serializable]
    public struct CloudSaveTrackedKey
    {
        public string Key;
        public CloudSaveTrackedValueType ValueType;
    }

    [CreateAssetMenu(menuName = "TapEmpire/Settings/CloudSaveSettings", fileName = "CloudSaveSettings")]
    public class CloudSaveSettings : ScriptableObject
    {
        [Tooltip("Automatically save to cloud when the app loses focus.")]
        public bool SaveOnFocusLost = true;

        [Tooltip("Delay in seconds before saving after a change is detected.")]
        [Min(0.0f)]
        public float SaveDebounceSeconds = 1.0f;

        [Tooltip("Tracked keys with explicit value type. Use this list for cross-project compatibility.")]
        public CloudSaveTrackedKey[] TrackedKeyTypes = new[]
        {
            new CloudSaveTrackedKey { Key = nameof(ProgressIntProp.CompletedLevelCount), ValueType = CloudSaveTrackedValueType.Int },
            new CloudSaveTrackedKey { Key = nameof(ProgressIntProp.CyclesCompleted), ValueType = CloudSaveTrackedValueType.Int },
            new CloudSaveTrackedKey { Key = nameof(ProgressIntProp.TotalLevels), ValueType = CloudSaveTrackedValueType.Int },
            new CloudSaveTrackedKey { Key = nameof(ProgressIntProp.VisualProgress), ValueType = CloudSaveTrackedValueType.String },
            new CloudSaveTrackedKey { Key = nameof(ProgressBoolProp.DisableAds), ValueType = CloudSaveTrackedValueType.Bool },
            new CloudSaveTrackedKey { Key = ProgressServiceExtensions.TutorialProgressKey, ValueType = CloudSaveTrackedValueType.Int },
            new CloudSaveTrackedKey { Key = ProgressServiceExtensions.IapShowProgressKey, ValueType = CloudSaveTrackedValueType.String }
        };
    }
}

using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/CloudSaveSettings", fileName = "CloudSaveSettings")]
    public class CloudSaveSettings : ScriptableObject
    {
        [Tooltip("Keys that should be excluded from cloud save/restore.")]
        public string[] ExcludedKeys = Array.Empty<string>();

        [SerializeField] private CloudSaveRestoreUIViewBase _restoreUIViewPrefab;
        public CloudSaveRestoreUIViewBase RestoreUIViewPrefab => _restoreUIViewPrefab;
    }
}

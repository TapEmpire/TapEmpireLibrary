using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class AddressableUtility
    {
        public static AddressableAssetEntry CreateAssetEntry<T>(T source) where T : Object
        {
            if (source == null || !AssetDatabase.Contains(source))
            {
                Debug.LogWarning($"Asset was not marked as Addressable");
                return null;
            }

            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            var entry = addressableSettings.CreateOrMoveEntry(sourceGuid, addressableSettings.DefaultGroup);
            entry.address = sourcePath;

            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

            return entry;
        }
    }
}

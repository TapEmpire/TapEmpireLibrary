using System;

namespace TapEmpire.Services
{
    // Mid-level save state. Games store their own payload by deriving from LevelSaveData;
    // the library only ever reads the base part, so it stays game agnostic.
    public static partial class ProgressServiceExtensions
    {
        public const string LevelSaveDataKey = "LevelSaveData";

        public static T GetLevelSaveData<T>(this IProgressService self) where T : LevelSaveData, new()
        {
            return self.HasLevelSaveData() ? self.GetSerializableObject<T>(LevelSaveDataKey) : null;
        }

        public static void SetLevelSaveData<T>(this IProgressService self, T data) where T : LevelSaveData
        {
            self.SetSerializableObject(LevelSaveDataKey, data);
        }

        // Cleared by writing an empty value rather than deleting the key, so that the cleared
        // state is part of the cloud save snapshot and cannot be undone by an import.
        public static void CleanLevelSaveData(this IProgressService self)
        {
            self.StringValuesDictionary.SetValue(LevelSaveDataKey, string.Empty);
        }

        public static bool HasLevelSaveData(this IProgressService self)
        {
            return self.StringValuesDictionary.TryGetValue(LevelSaveDataKey, out var value, canUseDefault: false)
                && !string.IsNullOrEmpty(value);
        }

        public static string GetLevelSaveName(this IProgressService self)
        {
            return self.GetLevelSaveData<LevelSaveData>()?.LevelName ?? string.Empty;
        }
    }

    [Serializable]
    public class LevelSaveData
    {
        public string LevelName;
    }
}

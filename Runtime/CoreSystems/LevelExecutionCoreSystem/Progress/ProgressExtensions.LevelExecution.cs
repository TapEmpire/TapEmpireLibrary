using System;

namespace TapEmpire.Services
{
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

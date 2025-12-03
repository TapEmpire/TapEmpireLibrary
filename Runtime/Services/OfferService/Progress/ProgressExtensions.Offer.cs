using System.Collections.Generic;
using Newtonsoft.Json;

namespace TapEmpire.Services.Offer
{
    public static partial class ProgressServiceExtensions
    {
        private static string RarityKey = "Rarity";
        private static string RaritySequenceKey = "RaritySequence";

        public static Rarity GetRarity(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(RarityKey, out var value) ? (Rarity)value : default;
        }

        public static void SetRarity(this IProgressService self, Rarity value)
        {
            self.IntValuesDictionary.SetValue(RarityKey, (int)value);
        }

        public static void CleanRarityData(this IProgressService self)
        {
            self.IntValuesDictionary.DeleteKey(RarityKey);
        }

        public static List<int> GetRaritySequence(this IProgressService self)
        {
            return self.GetSerializableObject<List<int>>(RaritySequenceKey);
        }

        public static void SetRaritySequence(this IProgressService self, List<int> data)
        {
            self.SetSerializableObject(RaritySequenceKey, data);
        }
        
        public static void ClearRaritySequence(this IProgressService self)
        {
            self.StringValuesDictionary.DeleteKey(RaritySequenceKey);
        }
    }
}

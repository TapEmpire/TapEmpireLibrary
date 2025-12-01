namespace TapEmpire.Services.Offer
{
    public static partial class ProgressServiceExtensions
    {
        private static string RarityKey = "Rarity";

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
    }
}

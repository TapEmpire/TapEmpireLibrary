namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public const string PurchasesTag = "Purchases";

        public static int GetPurchases(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(PurchasesTag, out var value) ? value : default;
        }

        public static bool HasPurchases(this IProgressService self)
        {
            return self.GetPurchases() > 0;
        }

        public static void AddPurchase(this IProgressService self)
        {
            self.UpdateInt(PurchasesTag);
        }
    }
}
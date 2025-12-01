using System;
using Newtonsoft.Json;

namespace TapEmpire.Services.Shop
{
    public static partial class ProgressServiceExtensions
    {
        private static string OfferDataKey = "OfferData";

        public static SaveOfferData GetOfferData(this IProgressService self)
        {
            if (self.StringValuesDictionary.TryGetValue(OfferDataKey, out var value, canUseDefault: false))
            {
                return JsonConvert.DeserializeObject<SaveOfferData>(value);
            }

            return new();
        }

        public static void SetOfferData(this IProgressService self, string key, DateTime timeStamp)
        {
            var save = new SaveOfferData() {
                Key = key,
                TimeStamp = timeStamp,
            };

            var saveJson = JsonConvert.SerializeObject(save);
            self.StringValuesDictionary.SetValue(OfferDataKey, saveJson);
        }

        public static void CleanOfferData(this IProgressService self)
        {
            self.StringValuesDictionary.DeleteKey(OfferDataKey);
        }
    }

    [Serializable]
    public struct SaveOfferData
    {
        public string Key;
        public DateTime TimeStamp;
    }
}

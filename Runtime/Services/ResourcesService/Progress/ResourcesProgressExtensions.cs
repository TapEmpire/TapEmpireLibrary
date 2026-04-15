using System;

namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        private static string ResourceKey = "Resource_";
        private static string TimeStampKey = "TimeStamp_";

        public static string CreateResourceKey(string resourceName) => $"{ResourceKey}{resourceName}";
        private static string CreateTimeStampKey(string resourceName) => $"{TimeStampKey}{resourceName}";
        
        public static int GetResourceCount(this IProgressService self, string resourceName, int defaultValue = default)
        {
            var key = CreateResourceKey(resourceName);
            return self.IntValuesDictionary.TryGetValue(key, out var value, canUseDefault: false) ? value : defaultValue;
        }
        
        public static void SetResourceCount(this IProgressService self, string resourceName, int value)
        {
            var key = CreateResourceKey(resourceName);
            self.IntValuesDictionary.SetValue(key, value);
        }

        public static DateTime GetResourceTimeStamp(this IProgressService self, string resourceName)
        {
            return self.GetTimeStamp(CreateTimeStampKey(resourceName));
        }

        public static void SetResourceTimeStamp(this IProgressService self, string resourceName)
        {
            self.SetCurrentTimeStamp(CreateTimeStampKey(resourceName));
        }

        public static void SetResourceTimeStamp(this IProgressService self, string resourceName, DateTime dateTime)
        {
            self.SetTimeStamp(CreateTimeStampKey(resourceName), dateTime);
        }

        public static void CleanResourceTimeStamp(this IProgressService self, string resourceName)
        {
            self.CleanTimeStamp(CreateTimeStampKey(resourceName));
        }
    }
}

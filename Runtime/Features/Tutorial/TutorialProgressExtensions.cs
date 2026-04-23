using TapEmpire.Utility;

namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        public static string TutorialProgressKey = "TutorialProgress";

        public static int GetTutorialProgress(this IProgressService self)
        {
            return self.IntValuesDictionary.TryGetValue(TutorialProgressKey, out var value, true, false) ? value : default;
        }
        
        public static void SetTutorialProgress(this IProgressService self, int value)
        {
            self.IntValuesDictionary.SetValue(TutorialProgressKey, value);
        }
        
        public static int GetTutorialProgress(this IProgressService self, string postfix)
        {
            return self.IntValuesDictionary.TryGetValue($"{TutorialProgressKey}{postfix}", out var value, true, false) ? value : default;
        }
        
        public static void SetTutorialProgress(this IProgressService self, string postfix, int value)
        {
            self.IntValuesDictionary.SetValue($"{TutorialProgressKey}{postfix}", value);
        }
    }
}

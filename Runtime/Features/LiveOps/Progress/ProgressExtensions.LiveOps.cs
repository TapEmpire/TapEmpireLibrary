using System.Collections.Generic;
using Newtonsoft.Json;

namespace TapEmpire.Services.LiveOps
{
    public static partial class ProgressServiceExtensions
    {
        private const string LiveOpsKey = "LiveOps";

        public static StateData GetLiveOpsValue(this IProgressService self, string postfix = "") =>
            self.GetSerializableObject<StateData>(GetLiveOpsKey(postfix));
        public static void SetLiveOpsValue(this IProgressService self, StateData value, string postfix = "") =>
            self.SetSerializableObject(GetLiveOpsKey(postfix), value);
        public static void ClearLiveOpsValue(this IProgressService self, string postfix = "") =>
            self.StringValuesDictionary.DeleteKey(GetLiveOpsKey(postfix));

        private static string GetLiveOpsKey(string postfix) => $"{LiveOpsKey}{postfix}";
    }
}

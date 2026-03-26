using System.Text;
using UnityEngine;

namespace TapEmpire.Services
{
    public static class ProgressSnapshotExtensions
    {
        public static void LogContents(this ProgressSnapshot snapshot, string tag = "[CloudSave]")
        {
            if (snapshot == null)
            {
                Debug.Log($"{tag} Snapshot is null.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{tag} Snapshot contents (UpdatedAt={snapshot.UpdatedAtUnixMs}, DeviceId={snapshot.DeviceId}):");

            if (snapshot.IntValues is { Count: > 0 })
            {
                sb.AppendLine($"  IntValues ({snapshot.IntValues.Count}):");
                foreach (var pair in snapshot.IntValues)
                {
                    sb.AppendLine($"    {pair.Key} = {pair.Value}");
                }
            }

            if (snapshot.BoolValues is { Count: > 0 })
            {
                sb.AppendLine($"  BoolValues ({snapshot.BoolValues.Count}):");
                foreach (var pair in snapshot.BoolValues)
                {
                    sb.AppendLine($"    {pair.Key} = {pair.Value}");
                }
            }

            if (snapshot.StringValues is { Count: > 0 })
            {
                sb.AppendLine($"  StringValues ({snapshot.StringValues.Count}):");
                foreach (var pair in snapshot.StringValues)
                {
                    sb.AppendLine($"    {pair.Key} = {pair.Value}");
                }
            }

            Debug.Log(sb.ToString());
        }

        public static string GetVisualProgress(this ProgressSnapshot snapshot)
        {
            var key = nameof(ProgressIntProp.VisualProgress);
            if (snapshot?.StringValues != null &&
                snapshot.StringValues.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        public static int GetResourceCount(this ProgressSnapshot snapshot, string resourceName, int defaultValue = 0)
        {
            var key = ProgressServiceExtensions.CreateResourceKey(resourceName);
            if (snapshot?.IntValues != null &&
                snapshot.IntValues.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}

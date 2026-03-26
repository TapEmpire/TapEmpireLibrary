namespace TapEmpire.Services
{
    public static class ProgressSnapshotUtility
    {
        public static bool IsEmpty(ProgressSnapshot snapshot)
        {
            return snapshot == null ||
                (snapshot.IntValues?.Count ?? 0) == 0 &&
                (snapshot.BoolValues?.Count ?? 0) == 0 &&
                (snapshot.StringValues?.Count ?? 0) == 0;
        }
    }
}

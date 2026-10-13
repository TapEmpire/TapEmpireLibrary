namespace TapEmpire.Services
{
    public static partial class ProgressServiceExtensions
    {
        private const string MusicTrackKey = "MusicTrack";

        public static int GetMusicTrack(this IProgressService self) => self.GetInt(MusicTrackKey);
        public static void SetMusicTrack(this IProgressService self, int value) => self.SetInt(MusicTrackKey, value);
    }
}

namespace TapEmpire.Utility
{
    public static class StringExtensions
    {
        public static string RemoveLastOccurence(this string str, string substring)
        {
            int lastIndex = str.LastIndexOf(substring);
            return lastIndex != -1 ? str.Remove(lastIndex, substring.Length) : str;
        }
    }
}
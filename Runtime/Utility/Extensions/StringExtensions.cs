using System.Linq;
using System.Text.RegularExpressions;

namespace TapEmpire.Utility
{
    public static class StringExtensions
    {
        public static string RemoveLastOccurence(this string str, string substring)
        {
            int lastIndex = str.LastIndexOf(substring);
            return lastIndex != -1 ? str.Remove(lastIndex, substring.Length) : str;
        }

        public static string RemoveFirstBlock(this string str, char symbol)
        {
            return str.Substring(str.IndexOf(symbol) + 1);
        }

        public static string Escape(this string str)
        {
            return System.Text.RegularExpressions.Regex.Escape(str);
        }

        // Escape for GoogleDocs
        public static string EscapeGD(this string str)
        {
            return str
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string ToTitleCase(this string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public static string ToCamelCase(this string str)
        {
            var strings = str.Split(' ').Select(subword => subword.Capitalize());
            return string.Join("", strings);
        }

        public static string ToCamelCaseWithSpaces(this string str)
        {
            var strings = str.Split(' ').Select(subword => subword.Capitalize());
            return string.Join(" ", strings);
        }

        public static string SplitByCapitalLetters(this string str)
        {
            return Regex.Replace(str, "(?<!^)([A-Z0-9])", " $1");
        }

        public static string FormatPlural(this string str, int amount)
        {
            return string.Format(str, amount > 1 ? "s" : "");
        }
    }
}
using System;
using System.Globalization;

namespace SiLanguage
{
    public static class Extensions
    {
        public static bool StartsWithIgnoreCase(this string text, string check)
        {
            return text.StartsWith(check, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWith(this string text, string check, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return text.StartsWith(check, comparison);
        }

        public static bool EndsWithIgnoreCase(this string text, string check)
        {
            return text.EndsWith(check, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToTitleCase(this string text)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
        }
    }
}

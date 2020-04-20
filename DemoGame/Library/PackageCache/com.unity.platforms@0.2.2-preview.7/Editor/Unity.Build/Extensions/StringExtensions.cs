namespace Unity.Build
{
    internal static class StringExtensions
    {
        public static string TrimStart(this string str, string value)
        {
            var index = str.IndexOf(value);
            return index >= 0 ? str.Substring(index + value.Length) : str;
        }

        public static string ToForwardSlash(this string value)
        {
            return value.Replace('\\', '/');
        }

        public static string SingleQuotes(this string value)
        {
            return "'" + value.Trim('\'') + "'";
        }

        public static string DoubleQuotes(this string value)
        {
            return '"' + value.Trim('"') + '"';
        }

        public static string ToHyperLink(this string value, string key = null)
        {
            return string.IsNullOrEmpty(key) ? $"<a>{value}</a>" : $"<a {key}={value.DoubleQuotes()}>{value}</a>";
        }
    }
}

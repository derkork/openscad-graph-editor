using System;
using System.Text;

namespace OpenScadGraphEditor.Utils
{
    public static class StringExt
    {
        private const string IndentString = "    ";

        public static string Indent(this string source)
        {
            return IndentString + source.Replace("\n", "\n" + IndentString);
        }

        public static bool ContainsIgnoreCase(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) > -1;
        }

        public static string AppendLines(this string source, params string[] lines)
        {
            if (lines.Length == 0)
            {
                return source;
            }

            var builder = new StringBuilder(source);
            foreach (var line in lines)
            {
                if (line.Length <= 0)
                {
                    continue;
                }

                builder.AppendLine();
                builder.Append(line);
            }

            return builder.ToString();
        }
    }
}
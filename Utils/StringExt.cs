using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenScadGraphEditor.Utils
{
    public static class StringExt
    {
        private const string IndentString = "    ";

        public static string Indent(this string source)
        {
            return IndentString + source.Replace("\n", "\n" + IndentString);
        }

        public static string PrefixUnlessEmpty(this string source, string prefix)
        {
            if (source.Length == 0)
            {
                return source;
            }

            return prefix + source;
        }

        public static bool ContainsIgnoreCase(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) > -1;
        }

        public static string AsBlock(this string input)
        {
            return input.Length == 0 ? input : $" {{\n{input.Indent()}\n}}\n";
        }

        public static string UniqueStableVariableName(this string id, int index)
        {
            
            return $"var{index}{Regex.Replace(Convert.ToBase64String(Guid.Parse(id).ToByteArray()), "[/+=]", "")}";
        }
    }
}
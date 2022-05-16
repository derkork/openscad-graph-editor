using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace OpenScadGraphEditor.Utils
{
    public static class StringExt
    {
        private const string IndentString = "    ";

        public static string Indent(this string source)
        {
            while (true)
            {
                // do not indent trailing newlines
                if (!source.EndsWith("\n"))
                {
                    return IndentString + source.Replace("\n", "\n" + IndentString);
                }
                source = source.Substring(0, source.Length - 1);
            }
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
            return input.Length == 0 ? ";" : $" {{\n{input.Indent()}\n}}\n";
        }

        public static string UniqueStableVariableName(this string id, int index)
        {
            
            return $"var{index}__{Regex.Replace(Convert.ToBase64String(Guid.Parse(id).ToByteArray()), "[/+=]", "")}";
        }
        
        public static string OrUndef(this string input)
        {
            return input.Length == 0 ? "undef" : input;
        }

        public static string OrDefault(this string input, string defaultValue)
        {
            return input.Length == 0 ? defaultValue : input;
        }
        
        public static string WordWrap(this string input, int maxCharactersPerLine)
        {            
            var lines = input.Split('\n');
            var output = "";
            foreach (var line in lines)
            {
                var words = line.Split(' ');
                var currentLine = "";
                foreach (var word in words)
                {
                    if (currentLine.Length + word.Length > maxCharactersPerLine)
                    {
                        output += currentLine + "\n";
                        currentLine = word;
                    }
                    else
                    {
                        if (currentLine.Empty())
                        {
                            currentLine = word;
                        }
                        else
                        {
                            currentLine += " " + word;
                        }
                    }
                }

                output += currentLine + "\n";
            }
            return output;
        }

        /// <summary>
        /// Prefixes all lines in the given string with the given prefix.
        /// </summary>
        public static string PrefixLines(this string input, string prefix)
        {
            return input.Split('\n')
                .Select(line => prefix + line)
                .JoinToString("\n");
        }

    }
}
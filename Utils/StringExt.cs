using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Utils
{
    public static class StringExt
    {
        private const string INDENT_STRING = "    ";

        private static string Indent(this string source)
        {
            while (true)
            {
                // do not indent trailing newlines
                if (!source.EndsWith("\n"))
                {
                    return INDENT_STRING + source.Replace("\n", "\n" + INDENT_STRING);
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

        public static string Trimmed(this string input, int maxLength)
        {
            if (input.Length <= maxLength)
            {
                return input;
            }

            if (maxLength < 3)
            {
                // maxLenght is super short, return maxLength dots
                return new string('.', maxLength);
            }

            return input.Substring(0, maxLength - 3) + "...";
        }

        public static string UniqueStableVariableName(this string id, int index)
        {
            GdAssert.That(index >= 0, "index must be >= 0");
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

        /// <summary>
        /// Word-wraps the given string to the given maximum number of characters per line.
        /// </summary>
        public static string WordWrap(this string input, int maxCharactersPerLine)
        {
            var lines = input.Split('\n');
            var output = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var words = line.Split(' ');
                var currentLine = "";
                foreach (var word in words)
                {
                    if (currentLine.Length + word.Length > maxCharactersPerLine)
                    {
                        output.Append(currentLine + "\n");
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

                output.Append(currentLine);
                if (i < lines.Length - 1)
                {
                   output.Append("\n");  // add newline after each line except the last one
                }
            }

            // if we have no visible output, return an empty string
            return output.IsNotBlank() ? output.ToString() : "";
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


        /// <summary>
        /// Checks if the given string is a valid identifier for an OpenSCAD variable.
        /// </summary>
        public static bool IsValidVariableIdentifier(this string input)
        {
            return !string.IsNullOrEmpty(input) &&
                   // variables can only contain letters, numbers and underscores, they may optionally start with $
                   // and the first character must not be a number
                   Regex.IsMatch(input, @"^\$?[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        /// <summary>
        /// Appends a newline to the given StringBuilder if it is not empty.
        /// </summary>
        public static StringBuilder NewLineUnlessBlank(this StringBuilder builder)
        {
            if (builder.IsNotBlank())
            {
                builder.Append("\n");
            }

            return builder;
        }

        /// <summary>
        /// Returns true if the given StringBuilder contains non-whitespace characters.
        /// </summary>
        public static bool IsNotBlank(this StringBuilder builder)
        {
            return builder.ToString().Trim().Length > 0;
        }
    }
}
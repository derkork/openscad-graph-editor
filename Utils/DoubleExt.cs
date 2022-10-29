using System.Globalization;

namespace OpenScadGraphEditor.Utils
{
    public static class DoubleExt
    {
        /// <summary>
        /// Parses a double in a safe, culture-invariant way.
        /// </summary>
        public static bool SafeTryParse(this string number, out double result)
        {
            return double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
        
        /// <summary>
        /// Parses a double in a safe, culture-invariant way.
        /// </summary>
        public static double SafeParse(this string number, double defaultValue = 0)
        {
            if (!number.SafeTryParse(out var result))
            {
                result = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a double to a string in a safe, culture-invariant way.
        /// </summary>
        public static string SafeToString(this double number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a double to a string in a safe, culture-invariant way.
        /// </summary>
        public static string SafeToString(this float number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
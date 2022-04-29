using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenScadGraphEditor.Utils
{
    public static class EnumerableExt
    {

        public static string JoinToString<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items.ToArray());
        }

        public static IEnumerable<int> Range(this int to, int from = 0, int step = 1)
        {
            for (var i = from; i < to; i += step)
            {
                yield return i;
            }
        }
        
        public static void ForAll<T>(this IEnumerable<T> items, Action<T> perform)
        {
            foreach (var item in items)
            {
                perform(item);
            }
        }

        public static IEnumerable<int> Indices<T>(this IEnumerable<T> self)
        {
            var index = 0;
            foreach (var item in self)
            {
                yield return index;
                index++;
            }
        }
    }
}
using System;
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
            foreach (var _ in self)
            {
                yield return index;
                index++;
            }
        }
        
        // minBy (as it is only available in .NET 6)
        public static T MinBy<T, TKey>(this IEnumerable<T> self, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            return self.Aggregate((a, b) => keySelector(a).CompareTo(keySelector(b)) < 0 ? a : b);
        }
        
        // maxBy (as it is only available in .NET 6)
        public static T MaxBy<T, TKey>(this IEnumerable<T> self, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            return self.Aggregate((a, b) => keySelector(a).CompareTo(keySelector(b)) > 0 ? a : b);
        }
    }
}
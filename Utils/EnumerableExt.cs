using System;
using System.Collections.Generic;

namespace OpenScadGraphEditor.Utils
{
    public static class EnumerableExt
    {
        public static void ForAll<T>(this IEnumerable<T> items, Action<T> perform)
        {
            foreach (var item in items)
            {
                perform(item);
            }
        }
    }
}
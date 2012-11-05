

namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static void AddAll<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static void ReplaceAll<T>(this IList<T> list, IEnumerable<T> items)
        {
            list.Clear();
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}

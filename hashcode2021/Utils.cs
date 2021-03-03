using System;
using System.Collections.Generic;
using System.Text;

namespace hashcode2021
{
    public static class Utils
    {
        public static void AddSorted<T>(this List<T> list, T item, IComparer<T> comparer)
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }
            if (comparer.Compare(list[list.Count - 1], item) <= 0)
            {
                list.Add(item);
                return;
            }
            if (comparer.Compare(list[0], item) >= 0)
            {
                list.Insert(0, item);
                return;
            }

            int index = list.BinarySearch(item);
            if (index < 0)
                index = ~index;
            list.Insert(index, item);
        }
    }
}

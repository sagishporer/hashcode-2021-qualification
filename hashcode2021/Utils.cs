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

            int index = list.BinarySearch(item, comparer);
            if (index < 0)
                index = ~index;
            list.Insert(index, item);
        }

        public static void SwapItems<T>(List<T> list, int item1, int item2)
        {
            T tmp = list[item1];
            list[item1] = list[item2];
            list[item2] = tmp;
        }

        public static void SwapItems<T>(List<T> list1, int item1, List<T> list2, int item2)
        {
            T tmp = list1[item1];
            list1[item1] = list2[item2];
            list2[item2] = tmp;
        }

        public static int IntersectSize(int[] a, int[] b)
        {
            int size = 0;
            int posA = 0;
            int posB = 0;

            while ((posA < a.Length) && (posB < b.Length))
            {
                if (a[posA] < b[posB])
                    posA++;
                else if (a[posA] > b[posB])
                    posB++;
                else if (a[posA] == b[posB])
                {
                    size++;
                    posA++;
                    posB++;
                }
                else
                    throw new Exception("Ha?");
            }

            return size;
        }

        public static int IntersectSize<T>(HashSet<T> a, HashSet<T> b)
        {
            HashSet<T> small;
            HashSet<T> big;
            if (a.Count < b.Count)
            {
                small = a;
                big = b;
            }
            else
            {
                small = b;
                big = a;
            }

            int size = 0;
            // Iterate on the smaller HashSet (faster)
            foreach (T val in small)
                if (big.Contains(val))
                    size++;

            return size;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
    public static class Utility
    {
        // This function returns all integers in the sequence { 0, 1, 2, ... , n } that are not contained in the
        // arr parameter
        public static int[] ComplementIndicesArray(int[] arr, int n)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (!arr.Contains(i))
                {
                    output.Add(i);
                }
            }
            return output.ToArray();
        }

        public static string DefaultString<T>(this IEnumerable<T> arr)
        {
            return CollectionToString(arr, "{", ", ", "}");
        }
        public static string CollectionToString<T>(IEnumerable<T> arr, string start, string seperator, string end)
        {
            return start + String.Join(seperator, arr) + end;
        }


        // This function returns a new array where the elements represent their index in the input array. If we
        // think of the input array as a map this function reverses the map.
        public static int[] InvertIntArray(int[] arr)
        {
            int[] output = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                output[i] = -1;
            }
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] >= 0)
                {
                    output[arr[i]] = i;
                }
            }
            return output;
        }

        public static IEnumerable<IEnumerable<T>> Subset<T>(this IEnumerable<T> elements)
        {
            int count = elements.Count();
            if (count == 1)
            {
                yield return elements.Take(0);
                yield return elements.Take(1);
            }
            else
            {
                List<int> output = new List<int>();

                foreach (var remainingSubset in Subset(elements.Skip(1)))
                {
                    yield return remainingSubset;
                    yield return elements.Take(1).Concat(remainingSubset);
                }
            }
        }
        public static IEnumerable<IEnumerable<T>> OrderedSubset<T>(this IEnumerable<T> elements)
        {
            int count = elements.Count();
            if (count == 1)
            {
                yield return elements.Take(0);
                yield return elements;
            }
            else
            {
                List<int> output = new List<int>();

                foreach (var remainingSubset in OrderedSubset(elements.Skip(1)))
                {
                    yield return remainingSubset;

                    int subCount = remainingSubset.Count();

                    for (int i = 0; i < subCount + 1; i++)
                    {
                        yield return remainingSubset.Take(i).Concat(elements.Take(1)).Concat(remainingSubset.Skip(i));
                    }
                }
            }
        }
        public static IEnumerable<IEnumerable<T>> Permutation<T>(this IEnumerable<T> elements)
        {
            int count = elements.Count();
            if (count == 1)
            {
                yield return elements;
            }
            else
            {
                foreach (var item in Permutation<T>(elements.Skip(1)))
                {
                    for (int i = count - 1; i >= 0; i--)
                    {
                        yield return item.Take(i).Concat(elements.Take(1)).Concat(item.Skip(i));
                    }
                }
            }
        }
    }
}

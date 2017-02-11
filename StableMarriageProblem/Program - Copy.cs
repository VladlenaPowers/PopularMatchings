using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StableMarriageProblem
{
    class Program
    {
        private static string CollectionToString<T>(IEnumerable<T> arr)
        {
            return "{" + String.Join(", ", arr) + "}";
        }

        private static IEnumerable<IEnumerable<T>> Permutation<T>(IEnumerable<T> elements)
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

        //this algorithm generates a collection of matchings
        private static IEnumerable<int[]> GenerateValidMatchings(int[][] men, int[][] women)
        {
            foreach (var matching in Permutation(Enumerable.Range(0, men.Length)))
            {
                int[] cpy = matching.ToArray();
                for (int i = 0; i < men.Length; i++)
                {
                    int woman = cpy[i];
                    if (woman >= 0 && (!women[woman].Contains(i) || !men[i].Contains(woman)))
                    {
                        cpy[i] = -1;
                    }
                }
                yield return cpy;
            }
        }

        //returns all of the popular matchings for a given set of matchings
        private static IEnumerable<int[]> PopularMatchings(int[][] men, int[][] women, IEnumerable<int[]> matchings)
        {
            List<int[]> output = new List<int[]>();
            int[][] matchingsArr = matchings.ToArray();
            Matching.PopularityComparer comparer = new Matching.PopularityComparer(men, women);
            for (int i = 0; i < matchingsArr.Length; i++)
            {
                bool popular = true;
                for (int j = 0; j < matchingsArr.Length; j++)
                {
                    if (i != j)
                    {
                        if (comparer.Compare(matchingsArr[i], matchingsArr[j]) < 0)
                        {
                            popular = false;
                            break;
                        }
                    }
                }
                if (popular)
                {
                    yield return matchingsArr[i];
                }
            }
        }

        static void Main(string[] args)
        {
            foreach (var permutation in Permutation(Enumerable.Range(0, 3)))
            {
                Console.WriteLine(CollectionToString<int>(permutation));
            }

            Console.WriteLine(Permutation(Enumerable.Range(0, 10)).Count());

            Console.Read();
        }
    }
}

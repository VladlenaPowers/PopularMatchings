using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StableMarriageProblem
{

    public static class Program
    {
        private static string DefaultString<T>(this IEnumerable<T> arr)
        {
            return CollectionToString(arr, "{", ", ", "}");
        }
        private static string CollectionToString<T>(IEnumerable<T> arr, string start, string seperator, string end)
        {
            return start + String.Join(seperator, arr) + end;
        }

        private static IEnumerable<IEnumerable<T>> Subset<T>(this IEnumerable<T> elements)
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
        private static IEnumerable<IEnumerable<T>> Permutation<T>(this IEnumerable<T> elements)
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
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(128, 64);

            int[][] men = new int[8][]
             {  new int[5] { 4,0,1,5,7 },
                new int[6] { 1,4,3,0,7,5 },
                new int[6] { 7,4,0,3,5,1 },
                new int[0] { },
                new int[7] { 6,1,4,0,5,7,3 },
                new int[6] { 0,5,4,7,3,1 },
                new int[2] { 4,6 },
                new int[6] { 7,3,4,1,5,0 }
             };
            int[][] women = new int[8][]
            {   new int[6] { 4,0,1,5,7,2 },
                new int[7] { 1,2,4,3,0,7,5 },
                new int[7] { 7,4,0,3,5,1,2 },
                new int[5] { 2,1,5,7,4 },
                new int[8] { 6,1,4,0,2,5,7,3 },
                new int[7] { 0,5,4,7,3,1,2 },
                new int[2] { 4,6 },
                new int[7] { 2,7,3,4,1,5,0 }
            };
            
            if(true)
            {
                //Run Kavitha's Algorithm
                Dictionary<int[], List<int[]>> matchingToMen1Dict = new Dictionary<int[], List<int[]>>(Matching.equalityComparer);
                foreach (var prioritizedMen in Subset(Enumerable.Range(0, men.Length)))
                {
                    KavithaAlgorithm.Output o = KavithaAlgorithm.Run(men, women, prioritizedMen);
                    if (matchingToMen1Dict.ContainsKey(o.matching))
                    {
                        matchingToMen1Dict[o.matching].Add(o.men1);
                    }
                    else
                    {
                        List<int[]> temp = new List<int[]>();
                        temp.Add(o.men1);
                        matchingToMen1Dict[o.matching] = temp;
                    }
                }

                foreach (var kvPair in matchingToMen1Dict)
                {
                    Console.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                    Console.WriteLine(CollectionToString(kvPair.Value.Select(list => list.DefaultString()), "", "\n", ""));
                }
            }

            if(false)
            {
                IEnumerable<int[]> matchings = GenerateValidMatchings(men, women);
                matchings = matchings.Distinct(Matching.equalityComparer);

                matchings.Subset();

                foreach (var popularMatching in PopularMatchings(men, women, matchings))
                {
                    Console.WriteLine(popularMatching.DefaultString());
                }




                //foreach (var permutation in Permutation(Enumerable.Range(0, 3)))
                //{
                //    Console.WriteLine(CollectionToString(permutation));
                //}

                //Console.WriteLine(Permutation(Enumerable.Range(0, 10)).Count());
            }



            Console.Read();
        }
    }
}

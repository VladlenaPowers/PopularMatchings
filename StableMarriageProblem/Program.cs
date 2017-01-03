using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableMarriageProblem
{
    class Program
    {
        private static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        
        private static IEnumerable<int[]> Permutate_Aux(int[] arr, int iEnd)
        {
            if (iEnd == 0)
            {
                yield return arr;
                yield break;
            }
            
            foreach (var item in Permutate_Aux(arr, iEnd - 1))
            {
                yield return arr;
            }
            for (int i = 0; i < iEnd; i++)
            {
                Swap(ref arr[i], ref arr[iEnd]);
                foreach (var item in Permutate_Aux(arr, iEnd - 1))
                {
                    yield return arr;
                }
                Swap(ref arr[i], ref arr[iEnd]);
            }
        }
        private static IEnumerable<int[]> Permutate(int n)
        {
            int[] output = new int[n];
            for (int i = 0; i < n; i++)
            {
                output[i] = i;
            }

            foreach (var permutation in Permutate_Aux(output, n - 1))
            {
                yield return permutation;
            }
        }

        //Given an array permutate the non-negative elements {0, 1, 2, ... , count - 1} in place
        private static IEnumerable<int[]> PermutateNonNegative(int[] arr, int count)
        {
            foreach (var permutation in Permutate(count))
            {
                int j = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] >= 0)
                    {
                        arr[i] = permutation[j++];
                    }
                }

                yield return arr;
            }
        }

        private static IEnumerable<int[]> Choose(int n, int k)
        {
            if (n == k)
            {
                int[] output = new int[k];
                for (int i = 0; i < k; i++)
                {
                    output[i] = i;
                }
                yield return output;
            }
            else if (k > n)
            {
                yield break;
            }
            else if (k == 0)
            {
                yield return new int[0];
            }
            else
            {
                //Choose the first element and choose k - 1 in the remaining n - elements
                int[] output = new int[k];
                output[0] = 0;
                foreach (var subOut in Choose(n - 1, k - 1))
                {
                    for (int i = 1; i < k; i++)
                    {
                        int temp = subOut[i - 1];
                        temp++;
                        output[i] = temp;
                    }
                    yield return output;
                }

                foreach (var subOut in Choose(n - 1, k))
                {
                    for (int i = 0; i < subOut.Length; i++)
                    {
                        subOut[i]++;
                    }
                    yield return subOut;
                }
            }
        }

        private static int ComparePairings(int[] pref, int a, int b)
        {
            int aRank = -1;
            int bRank = -1;
            for (int i = 0; i < pref.Length; i++)
            {
                int test = pref[i];
                if (test == a)
                {
                    aRank = i;
                    if (bRank >= 0)
                    {
                        break;
                    }
                }
                if (test == b)
                {
                    bRank = i;
                    if (aRank >= 0)
                    {
                        break;
                    }
                }
            }

            bool containsA = aRank >= 0;
            bool containsB = bRank >= 0;
            if (containsA && containsB)
            {
                if (bRank < aRank)
                {
                    return -1;
                }
                else if (aRank < bRank)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else if(containsA)
            {
                return 1;
            }
            else if(containsB)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        static int CompareMatching(int[][] men, int[][] women, int[] a, int[] b)
        {
            //ask each person which matching they prefer

            int aScore = 0;
            int bScore = 0;

            for (int i = 0; i < a.Length; i++)
            {
                int manPref = ComparePairings(men[i], a[i], b[i]);
                aScore += Math.Max(0, manPref);
                bScore += Math.Max(0, -manPref);
            }

            for (int i = 0; i < women.Length; i++)
            {
                int indexA = -1;
                int indexB = -1;
                for (int j = 0; j < a.Length; j++)
                {
                    if (a[j] == i)
                    {
                        indexA = j;
                        if (indexB >= 0)
                        {
                            break;
                        }
                    }
                    if (b[j] == i)
                    {
                        indexB = j;
                        if (indexA >= 0)
                        {
                            break; 
                        }
                    }
                }
                int womanPref = ComparePairings(women[i], indexA, indexB);
                aScore += Math.Max(0, womanPref);
                bScore += Math.Max(0, -womanPref);
            }

            if (aScore < bScore)
            {
                return -1;
            }
            else if (bScore < aScore)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        static List<int[]> FindPopularMatchings(int[][] men, int[][] women, List<int[]> matchings)
        {
            List<int[]> output = new List<int[]>();
            for (int i = 0; i < matchings.Count; i++)
            {
                bool popular = true;
                for (int j = 0; j < matchings.Count; j++)
                {
                    if (i != j)
                    {
                        if (CompareMatching(men, women, matchings[i], matchings[j]) < 0)
                        {
                            popular = false;
                            break;
                        }
                    }
                }
                if (popular)
                {
                    output.Add(matchings[i]);
                }
            }
            return output;
        }

        private static void PrintIntArray(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0)
                {
                    Console.Write("{");
                }
                else
                {
                    Console.Write(", ");
                }
                Console.Write(arr[i]);
            }
            Console.Write("}\n");
        }
        
        static void Main(string[] args)
        {
            int[][] men = new int[4][]
            {
                new int[1] { 0 },
                new int[0] { },
                new int[2] { 0, 2 },
                new int[2] { 0, 1 }
            };

            int[][] women = new int[3][]
            {
                new int[1] { 3 },
                new int[2] { 3, 2 },
                new int[1] { 1 }
            };

            if (men.Length >= women.Length)
            {
                List<int[]> matchings = new List<int[]>();
                int[] vector = new int[men.Length];
                int unpairedMen = men.Length - women.Length;
                Console.WriteLine("Matchings");
                foreach (var unpairedMenIndex in Choose(men.Length, unpairedMen))
                {
                    int j = 0;
                    for (int i = 0; i < unpairedMenIndex.Length; i++)
                    {
                        while (j < unpairedMenIndex[i]) { vector[j++] = 0; }
                        vector[unpairedMenIndex[i]] = -1;
                        j++;
                    }
                    while (j < men.Length) { vector[j++] = 0; }

                    foreach (var matching in PermutateNonNegative(vector, women.Length))
                    {

                        PrintIntArray(matching);
                        matchings.Add(matching.ToArray());
                    }
                }
                Console.WriteLine("Popular");
                List<int[]> popularMatchings = FindPopularMatchings(men, women, matchings);
                foreach (var popularMatching in popularMatchings)
                {
                    PrintIntArray(popularMatching);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            Console.Read();
        }
    }
}

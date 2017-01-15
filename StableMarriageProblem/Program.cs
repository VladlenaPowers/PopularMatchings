﻿using System;
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

        private static void Walk(int currentIndex, int k, int n, List<int> indexesSoFar, List<int[]> choosings)
        {
            if (indexesSoFar.Count == k)
            {
                choosings.Add(indexesSoFar.ToArray());
            }
            else if(currentIndex != n)
            {
                List<int> cpy = new List<int>(indexesSoFar);
                cpy.Add(currentIndex);
                Walk(currentIndex + 1, k, n, cpy, choosings);
                Walk(currentIndex + 1, k, n, indexesSoFar, choosings);
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
            Console.Write("}");
        }
        private static void PrintIntIntArray(List<int[]> arr)
        {
            bool first0 = false;
            foreach (var arr2 in arr)
            {
                if(!first0)
                {
                    Console.Write("{");
                    first0 = true;
                }
                else
                {
                    Console.Write(",\n");
                }
                bool first1 = false;
                foreach (var item in arr2)
                {
                    if (!first1)
                    {
                        Console.Write("{");
                        first1 = true;
                    }
                    else
                    {
                        Console.Write(", ");
                    }
                    Console.Write(item);
                }
                Console.Write("}");
            }
            Console.Write("}");
        }
        private static void PrintIndexPairArray(IndexPair[] arr)
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
                Console.Write(arr[i].b);
            }
            Console.Write("}\n");
        }

        struct IndexPair
        {
            public int a, b;
            public IndexPair(int a, int b)
            {
                this.a = a;
                this.b = b;
            }
        }

        private static int[] KavithaAlgorithm(int[][] men, int[][] women, int[] prioritizedMen)
        {
            List<int>[][] doubleMen = new List<int>[2][];
            for (int i = 0; i < 2; i++)
            {
                doubleMen[i] = new List<int>[men.Length];
                for (int j = 0; j < men.Length; j++)
                {
                    doubleMen[i][j] = new List<int>(men[j].Length);
                    for (int l = 0; l < men[j].Length; l++)
                    {
                        doubleMen[i][j].Add(men[j][l]);
                    }
                }
            }
            List<int>[] copyWomen = new List<int>[women.Length];
            for (int i = 0; i < women.Length; i++)
            {
                copyWomen[i] = new List<int>();
                for (int j = 0; j < women[i].Length; j++)
                {
                    copyWomen[i].Add(women[i][j]);
                }
            }
            Queue<IndexPair> queuedMen = new Queue<IndexPair>();
            for (int i = 0; i < prioritizedMen.Length; i++)
            {
                queuedMen.Enqueue(new IndexPair(1, prioritizedMen[i]));
            }
            for (int i = 0; i < doubleMen[0].Length; i++)
            {
                if (!prioritizedMen.Contains(i))
                {
                    queuedMen.Enqueue(new IndexPair(0, i));
                }    
            }
            IndexPair[] kavithaMatching = new IndexPair[men.Length];
            for (int i = 0; i < men.Length; i++)
            {
                kavithaMatching[i] = new IndexPair(0, -1);
            }
            //kavithaMatching[7] = new IndexPair(1, 2);
            //for (int j = 7 - 1; j > 0; j--)
            //{
            //    doubleMen[1][copyWomen[2][j]].Remove(2);
            //    doubleMen[0][copyWomen[2][j]].Remove(2);
            //    copyWomen[2].RemoveAt(j);
            //}

                    while (queuedMen.Count != 0)
            {
                IndexPair current = queuedMen.Dequeue();
                if (doubleMen[current.a][current.b].Count != 0)
                {
                    int mostPreferredNeighbor = doubleMen[current.a][current.b][0];
                    int matchedManI = -1;
                    for (int j = 0; j < kavithaMatching.Length; j++)
                    {
                        if(kavithaMatching[j].b == mostPreferredNeighbor)
                        {
                            matchedManI = j;
                            break;
                        }
                    }
                    if (matchedManI >= 0)
                    {
                        queuedMen.Enqueue(new IndexPair(kavithaMatching[matchedManI].a, matchedManI));
                        kavithaMatching[matchedManI].b = -1;
                    }
                    kavithaMatching[current.b] = new IndexPair(current.a, mostPreferredNeighbor);
                    //PrintIndexPairArray(kavithaMatching);

                    List<int> womansPref = copyWomen[mostPreferredNeighbor];

                    int i = womansPref.IndexOf(current.b);
                    for (int j = womansPref.Count - 1; j > i; j--)
                    {
                        if (current.a == 0)
                        {
                            doubleMen[current.a][womansPref[j]].Remove(mostPreferredNeighbor);
                            womansPref.RemoveAt(j);
                        }
                        else
                        {
                            doubleMen[current.a][womansPref[j]].Remove(mostPreferredNeighbor);
                            doubleMen[0][womansPref[j]].Remove(mostPreferredNeighbor);
                            womansPref.RemoveAt(j);
                        }
                    }
                }
                else if (current.a == 0)
                {
                    queuedMen.Enqueue(new IndexPair(1, current.b));
                }
            }

            int[] output = new int[kavithaMatching.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = kavithaMatching[i].b;
            }
            return output;
        }

        static bool CompareIntArrays(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
        
        static void Main(string[] args)
        {
            int[][] men = new int[8][]
             {  new int[6] { 4,0,1,5,7,2 },
                new int[7] { 1,2,4,3,0,7,5 },
                new int[7] { 7,4,0,3,5,1,2 },
                new int[5] { 2,1,5,7,4 },
                new int[8] { 6,1,4,0,2,5,7,3 },
                new int[7] { 0,5,4,7,3,1,2 },
                new int[2] { 4,6 },
                new int[7] { 2,7,3,4,1,5,0 }
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

            List<int[]> uniqueOutputs = new List<int[]>();
            List<List<int[]>> prioritizedMen = new List<List<int[]>>();

            for (int i = 0; i < men.Length; i++)
            {
                List<int[]> choosings = new List<int[]>();
                Walk(0, i, men.Length, new List<int>(), choosings);
                
                foreach (var prioritizedMenI in choosings)
                {
                    int[] matching = KavithaAlgorithm(men, women, prioritizedMenI);

                    bool contained = false;
                    for (int j = 0; j < uniqueOutputs.Count; j++)
                    {
                        if (CompareIntArrays(matching, uniqueOutputs[j]))
                        {
                            prioritizedMen[j].Add(prioritizedMenI);
                            contained = true;
                            break;
                        }
                    }

                    if (!contained)
                    {
                        uniqueOutputs.Add(matching);
                        prioritizedMen.Add(new List<int[]>());
                        prioritizedMen.Last().Add(prioritizedMenI);
                    }
                }
            }

            for (int i = 0; i < uniqueOutputs.Count; i++)
            {
                Console.Write("------------------------");
                PrintIntArray(uniqueOutputs[i]);
                Console.Write("------------------------\n");
                PrintIntIntArray(prioritizedMen[i]);
                Console.WriteLine();
            }

            //if (men.Length >= women.Length)
            //{
            //    List<int[]> matchings = new List<int[]>();
            //    int[] vector = new int[men.Length];
            //    int unpairedMen = men.Length - women.Length;

            //    List<int[]> choosings = new List<int[]>();
            //    Walk(0, unpairedMen, men.Length, new List<int>(), choosings);

            //    foreach (var unpairedMenIndex in choosings)
            //    {
            //        for (int i = 0; i < vector.Length; i++)
            //        {
            //            vector[i] = 0;
            //        }
            //        for (int i = 0; i < unpairedMenIndex.Length; i++)
            //        {
            //            vector[unpairedMenIndex[i]] = -1;
            //        }

            //        PrintIntArray(vector);

            //        foreach (var matching in PermutateNonNegative(vector, women.Length))
            //        {
            //            int[] cpy = matching.ToArray();

            //            for (int i = 0; i < vector.Length; i++)
            //            {
            //                if (vector[i] < 0)
            //                {
            //                    Console.WriteLine("NEg");
            //                }
            //            }

            //            for (int i = 0; i < men.Length; i++)
            //            {
            //                int woman = cpy[i];
            //                if (woman >= 0 && (!women[woman].Contains(i) || !men[i].Contains(woman)))
            //                {
            //                    cpy[i] = -1;
            //                }
            //            }

            //            matchings.Add(cpy);
            //        }
            //    }
            //    Console.Write("Preferences lists\n");
            //    for (int i = 0; i < men.Length; i++)
            //    {
            //        Console.Write(i);
            //        Console.Write(": ");
            //        PrintIntArray(men[i]);
            //    }
            //    Console.Write("\n");
            //    Console.Write("\n");
            //    for (int i = 0; i < women.Length; i++)
            //    {
            //        Console.Write(i);
            //        Console.Write(": ");
            //        PrintIntArray(women[i]);
            //    }
            //    Console.Write("\n");
            //    Console.Write("\n");
            //    Console.WriteLine("Popular");
            //    List<int[]> popularMatchings = FindPopularMatchings(men, women, matchings);
            //    foreach (var popularMatching in popularMatchings)
            //    {
            //        PrintIntArray(popularMatching);
            //    }
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            Console.Read();
        }
    }
}

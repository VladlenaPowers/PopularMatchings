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
        
        private static void AddToArray(int[] arr, int a)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += a;
            }
        }

        private static IEnumerable<int[]> OrderedSubset(int n)
        {
            if (n == 1)
            {
                yield return new int[0] { };
                yield return new int[1] { 0 };
            }
            else
            {
                //insert ours into every orderedsubset
                List<int> output = new List<int>();

                foreach (int[] remainingSubset in OrderedSubset(n - 1))
                {
                    AddToArray(remainingSubset, 1);

                    //don't insert our element
                    output.AddRange(remainingSubset);
                    yield return output.ToArray();
                    output.Clear();

                    //insert our element into all possible places
                    for (int i = 0; i < remainingSubset.Length; i++)
                    {
                        for (int j = 0; j < remainingSubset.Length; j++)
                        {
                            if (i == j)
                            {
                                output.Add(0);
                                output.Add(remainingSubset[j]);
                            }
                            else
                            {
                                output.Add(remainingSubset[j]);
                            }
                        }

                        yield return output.ToArray();
                        output.Clear();
                    }

                    //append our element at the end
                    output.AddRange(remainingSubset);
                    output.Add(0);
                    yield return output.ToArray();
                    output.Clear();
                }
            }
        }
        private static IEnumerable<int[][][]> PreferenceListCombination(int n)
        {
            int[][] orderedSubsets = OrderedSubset(n).ToArray();
            int digitMax = orderedSubsets.Length - 1;

            int[][][] output = new int[2][][];
            output[0] = new int[n][];
            output[1] = new int[n][];

            for (int i = 0; i < output[0].Length; i++)
            {
                output[0][i] = orderedSubsets[0];
                output[1][i] = orderedSubsets[0];
            }

            Random r = new Random();

            int[] digits = new int[n * 2];
            int digitsTotal = digits.Length;
            int lastDigitI = digits.Length - 1;
            bool finished = false;
            while (!finished)
            {
                for (int i = 0; i < digitsTotal; i++)
                {
                    int g = i / n;
                    output[g][i - (g * n)] = orderedSubsets[r.Next(digitMax)];
                }

                yield return output;

                //for (int i = 0; i < digitsTotal; i++)
                //{
                //    digits[i]++;
                //    if (digits[i] == digitMax)
                //    {
                //        if (i == lastDigitI)
                //        {
                //            //overflow
                //            finished = true;
                //            break;
                //        }
                //        else
                //        {
                //            digits[i] = 0;
                //            int g = i / n;
                //            output[g][i - (g * n)] = orderedSubsets[0];
                //        }
                //    }
                //    else
                //    {
                //        int g = i / n;
                //        output[g][i - (g * n)] = orderedSubsets[digits[i]];
                //        break;
                //    }
                //}
            }
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
        static bool SymmetricPreferences(int[][] men, int[][] women)
        {
            for (int i = 0; i < men.Length; i++)
            {
                for (int j = 0; j < men[i].Length; j++)
                {
                    if (!women[men[i][j]].Contains(i))
                    {
                        return false;
                    }
                }
            }
            for (int i = 0; i < women.Length; i++)
            {
                for (int j = 0; j < women[i].Length; j++)
                {
                    if (!men[women[i][j]].Contains(i))
                    {
                        return false;
                    }
                }
            }

            return true;
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

        private static string CollectionToString<T>(ICollection<T> arr)
        {
            return "{" + String.Join(", ", arr) + "}";
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
        static IEnumerable<int[]> Union(ICollection<int[]> a, ICollection<int[]> b)
        {
            foreach (var item in a)
            {
                foreach (var otherItem in b)
                {
                    if(CompareIntArrays(item, otherItem))
                    {
                        yield return item;
                    }
                }
            }
        }
        static int NoNegativeEntries(int[] arr)
        {
            int output = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] >= 0)
                {
                    output++;
                }
            }
            return output;
        }

        struct IndexPair
        {
            public int a, b;
            public IndexPair(int a, int b)
            {
                this.a = a;
                this.b = b;
            }

            public override string ToString()
            {
                return a + ", " + b;
            }
        }
        
        private static int[] KavithaAlgorithm(int[][] men, int[][] women, int[] prioritizedMen, out int[] men1)
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
            PriorityQueue<IndexPair> queuedMen = new PriorityQueue<IndexPair>();
            
            for (int i = 0; i < prioritizedMen.Length; i++)
            {
                queuedMen.Enqueue(0, new IndexPair(1, prioritizedMen[i]));
            }
            for (int i = 0; i < doubleMen[0].Length; i++)
            {
                if (!prioritizedMen.Contains(i))
                {
                    queuedMen.Enqueue(1, new IndexPair(0, i));
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

            while (!queuedMen.Empty())
            {
                IndexPair current = queuedMen.Dequeue();
                //Console.WriteLine(current + "\n");
                if (doubleMen[current.a][current.b].Count != 0)
                {
                    int mostPreferredNeighbor = doubleMen[current.a][current.b][0];
                    //Console.WriteLine(mostPreferredNeighbor + "\n");
                    if (current.a == 1)
                    {
                        for (int j = 0; j < men.Length; j++)
                        {
                            //Console.WriteLine("remove in doulbemen0" + j + "woman" + mostPreferredNeighbor + "\n");
                            doubleMen[0][j].Remove(mostPreferredNeighbor);
                        }
                    }
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
                        queuedMen.Enqueue(1 - kavithaMatching[matchedManI].a, new IndexPair(kavithaMatching[matchedManI].a, matchedManI));
                        kavithaMatching[matchedManI].b = -1;
                    }
                    kavithaMatching[current.b] = new IndexPair(current.a, mostPreferredNeighbor);
                    //PrintIndexPairArray(kavithaMatching);

                     List<int> womansPref = copyWomen[mostPreferredNeighbor];

                    int i = womansPref.IndexOf(current.b);
                    //Console.WriteLine("womansPref.IndexOf " + i + "");
                    if (i == -1)
                    {
                        i = womansPref.Count - 1;
                    }
                    for (int j = womansPref.Count - 1; j > i; j--)
                    {
                        if (current.a == 0)
                        {
                            //Console.WriteLine("removing from doubleMen0 "+ womansPref[j] + " woman "+mostPreferredNeighbor + "");
                            //Console.WriteLine("removing from CopyWoman " + j + "");
                            doubleMen[current.a][womansPref[j]].Remove(mostPreferredNeighbor);
                            //womansPref.RemoveAt(j);
                        }
                        else
                        {
                            //Console.WriteLine("removing from doubleMen0 " + womansPref[j] + " woman " + mostPreferredNeighbor+"");
                            //Console.WriteLine("removing from doubleMen1 " + womansPref[j] + " woman " + mostPreferredNeighbor + "");
                            //Console.WriteLine("removing from CopyWoman " + j + "");
                            doubleMen[current.a][womansPref[j]].Remove(mostPreferredNeighbor);
                            doubleMen[0][womansPref[j]].Remove(mostPreferredNeighbor);
                            womansPref.RemoveAt(j);
                        }
                    }
                }
                else if (current.a == 0)
                {
                    queuedMen.Enqueue(0, new IndexPair(1, current.b));
                }
            }

            int[] output = new int[kavithaMatching.Length];
            List<int> _men1 = new List<int>();
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = kavithaMatching[i].b;
                if (kavithaMatching[i].a == 1)
                {
                    _men1.Add(i);
                }
            }
            men1 = _men1.ToArray();
            return output;
        }
        
        static void Main(string[] args)
        {
            foreach (var item in PreferenceListCombination(5))
            {

                int[][] men = item[0];
                int[][] women = item[1];

                if(!SymmetricPreferences(men, women))
                {
                    continue;
                }

                List<int[]> uniqueOutputs = new List<int[]>();
                List<List<int[]>> prioritizedMen = new List<List<int[]>>();
                List<List<int[]>> men1 = new List<List<int[]>>();

                for (int i = 0; i < men.Length; i++)
                {
                    List<int[]> choosings = new List<int[]>();
                    Walk(0, i, men.Length, new List<int>(), choosings);

                    foreach (var prioritizedMenI in choosings)
                    {
                        int[] _men1;
                        int[] matching = KavithaAlgorithm(men, women, prioritizedMenI, out _men1);

                        bool contained = false;
                        for (int j = 0; j < uniqueOutputs.Count; j++)
                        {
                            if (CompareIntArrays(matching, uniqueOutputs[j]))
                            {
                                prioritizedMen[j].Add(prioritizedMenI);
                                men1[j].Add(_men1);
                                contained = true;
                                break;
                            }
                        }

                        if (!contained)
                        {
                            uniqueOutputs.Add(matching);
                            prioritizedMen.Add(new List<int[]>());
                            men1.Add(new List<int[]>());
                            prioritizedMen.Last().Add(prioritizedMenI);
                            men1.Last().Add(_men1);
                        }
                    }
                }
                
                {
                    List<int[]> matchings = new List<int[]>();
                    int[] vector = new int[men.Length];
                    int unpairedMen = men.Length - women.Length;

                    List<int[]> choosings = new List<int[]>();
                    Walk(0, unpairedMen, men.Length, new List<int>(), choosings);

                    foreach (var unpairedMenIndex in choosings)
                    {
                        for (int i = 0; i < vector.Length; i++)
                        {
                            vector[i] = 0;
                        }
                        for (int i = 0; i < unpairedMenIndex.Length; i++)
                        {
                            vector[unpairedMenIndex[i]] = -1;
                        }

                        foreach (var matching in PermutateNonNegative(vector, women.Length))
                        {
                            int[] cpy = matching.ToArray();

                            for (int i = 0; i < vector.Length; i++)
                            {
                                if (vector[i] < 0)
                                {
                                    Console.WriteLine("NEg");
                                }
                            }

                            for (int i = 0; i < men.Length; i++)
                            {
                                int woman = cpy[i];
                                if (woman >= 0 && (!women[woman].Contains(i) || !men[i].Contains(woman)))
                                {
                                    cpy[i] = -1;
                                }
                            }

                            matchings.Add(cpy);
                        }
                    }
                    //Console.Write("Preferences lists\n");
                    //for (int i = 0; i < men.Length; i++)
                    //{
                    //    Console.Write(i);
                    //    Console.Write(": ");
                    //    Console.Write(CollectionToString(men[i]));
                    //    Console.Write("\n");
                    //}
                    //Console.Write("\n");
                    //Console.Write("\n");
                    //for (int i = 0; i < women.Length; i++)
                    //{
                    //    Console.Write(i);
                    //    Console.Write(": ");
                    //    Console.Write(CollectionToString(women[i]));
                    //    Console.Write("\n");
                    //}
                    //Console.Write("\n");
                    //Console.Write("\n");

                    //Console.WriteLine("Popular");

                    List<int[]> popularMatchings = FindPopularMatchings(men, women, matchings);

                    //foreach (var popularMatching in popularMatchings)
                    //{
                    //    Console.Write(CollectionToString(popularMatching));
                    //    Console.Write("\n");
                    //}

                    int[][] overlap = Union(popularMatchings, uniqueOutputs).ToArray();

                    var sizes = popularMatchings.Select(arr => NoNegativeEntries(arr)).ToArray();
                    Array.Sort(sizes);


                    if (sizes.Length > 1 && (sizes[0] != sizes[1]) && (overlap.Length != popularMatchings.Count))
                    {
                        Console.WriteLine("--------------------------------------------------------------------\n\n");

                        Console.WriteLine("int[][] men = new int[" + men.Length + "][]\n{");
                        bool first = true;
                        foreach (var man in men)
                        {
                            if(!first)
                            {
                                Console.WriteLine(",");
                            }
                            first = false;
                            Console.Write("new int[" + man.Length + "] " + CollectionToString(man));
                        }
                        Console.WriteLine("};");
                        Console.WriteLine("int[][] women = new int[" + women.Length + "][]\n{");
                        first = true;
                        foreach (var woman in women)
                        {
                            if (!first)
                            {
                                Console.WriteLine(",");
                            }
                            first = false;
                            Console.Write("new int[" + woman.Length + "] " + CollectionToString(woman));
                        }
                        Console.WriteLine("};");
                        //break;
                    }
                }
            }

            Console.Read();

           // int[][] men = new int[8][]
           //{    new int[1] { 0 },
           //     new int[2] { 0, 1 },
           //     new int[3] { 3, 1, 2 },
           //     new int[1] { 3 },
           //     new int[1] { 4 },
           //     new int[2] { 4, 5 },
           //     new int[3] { 7, 5, 6 },
           //     new int[1] { 7 }
           //};

           // int[][] women = new int[8][]
           // {   new int[2] { 1, 0 },
           //     new int[2] { 1, 2 },
           //     new int[1] { 2 },
           //     new int[2] { 2, 3 },
           //     new int[2] { 5, 4 },
           //     new int[2] { 5, 6 },
           //     new int[1] { 6 },
           //     new int[2] { 6, 7 }
           // };

            //KavithaAlgorithm(men, women, new int[5] { 1,2,5,6,7});

            //const string format = "{0,-32} :{1}";

            //for (int i = 0; i < uniqueOutputs.Count; i++)
            //{
            //    Console.Write("------------------------");
            //    Console.Write(CollectionToString(uniqueOutputs[i]));
            //    Console.WriteLine("------------------------");
            //    for (int j = 0; j < prioritizedMen[i].Count; j++)
            //    {
            //        Console.WriteLine(format, CollectionToString(prioritizedMen[i][j]), CollectionToString(men1[i][j]));
            //    }
            //    Console.WriteLine();
            //}

            //if (men.Length >= women.Length)
            //{
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            Console.Read();
        }
    }
}

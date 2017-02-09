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



        private static IEnumerable<int[]> Subset(int n)
        {
            if (n == 1)
            {
                yield return new int[0] { };
                yield return new int[1] { 0 };
            }
            else
            {
                List<int> output = new List<int>();

                foreach (int[] remainingSubset in OrderedSubset(n - 1))
                {
                    AddToArray(remainingSubset, 1);

                    //don't insert our element
                    output.AddRange(remainingSubset);
                    yield return output.ToArray();
                    output.Clear();

                    //append our element at the end
                    output.AddRange(remainingSubset);
                    output.Add(0);
                    yield return output.ToArray();
                    output.Clear();
                }
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
        private static IEnumerable<int[][][]> PreferenceListCombination(int n, int seed)
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

            Random r = new Random(seed);

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
            else if (currentIndex != n)
            {
                List<int> cpy = new List<int>(indexesSoFar);
                cpy.Add(currentIndex);
                Walk(currentIndex + 1, k, n, cpy, choosings);
                Walk(currentIndex + 1, k, n, indexesSoFar, choosings);
            }
        }

        private static int ComparePairings(int[] pref, int a, int b)
        {
            if (a == b)
            {
                return 0;
            }
            else if (a < 0)
            {
                return -1;
            }
            else if (b < 0)
            {
                return 1;
            }
            else
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

                if (bRank < aRank)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
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
                if (manPref > 0)
                {
                    aScore += 1;
                }
                else if (manPref < 0)
                {
                    bScore += 1;
                }
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
                if (womanPref > 0)
                {
                    aScore += 1;
                }
                else if (womanPref < 0)
                {
                    bScore += 1;
                }
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
                    if (CompareIntArrays(item, otherItem))
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
        static int[] InvertIntArray(int[] arr)
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
                        if (kavithaMatching[j].b == mostPreferredNeighbor)
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

        private static object Lock = new object();

        private static void RunAlgorithm(int[][] men, int[][] women, out List<int[]> popularMatchings, out List<int[]> uniqueMatchings, out List<List<int[]>> prioritizedMen, out List<List<int[]>> men1)
        {
            uniqueMatchings = new List<int[]>();
            prioritizedMen = new List<List<int[]>>();
            men1 = new List<List<int[]>>();

            for (int i = 0; i < men.Length; i++)
            {
                List<int[]> choosings = new List<int[]>();
                Walk(0, i, men.Length, new List<int>(), choosings);

                foreach (var prioritizedMenI in choosings)
                {
                    int[] _men1;
                    int[] matching = KavithaAlgorithm(men, women, prioritizedMenI, out _men1);

                    bool contained = false;
                    for (int j = 0; j < uniqueMatchings.Count; j++)
                    {
                        if (CompareIntArrays(matching, uniqueMatchings[j]))
                        {
                            prioritizedMen[j].Add(prioritizedMenI);
                            men1[j].Add(_men1);
                            contained = true;
                            break;
                        }
                    }

                    if (!contained)
                    {
                        uniqueMatchings.Add(matching);
                        prioritizedMen.Add(new List<int[]>());
                        men1.Add(new List<int[]>());
                        prioritizedMen.Last().Add(prioritizedMenI);
                        men1.Last().Add(_men1);
                    }
                }
            }

            List<int[]> matchings = new List<int[]>();
            int[] vector = new int[men.Length];
            int unpairedMen = men.Length - women.Length;

            List<int[]> chooses = new List<int[]>();
            Walk(0, unpairedMen, men.Length, new List<int>(), chooses);

            foreach (var unpairedMenIndex in chooses)
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

            popularMatchings = FindPopularMatchings(men, women, matchings);
        }

        static void Main_Aux(object seed)
        {
            foreach (var item in PreferenceListCombination(5, (int)seed))
            {
                int[][] men = item[0];
                int[][] women = item[1];

                if (!SymmetricPreferences(men, women))
                {
                    continue;
                }

                List<int[]> popularMatchings;
                List<int[]> uniqueMatchings;
                List<List<int[]>> prioritizedMen;
                List<List<int[]>> men1;
                RunAlgorithm(men, women, out popularMatchings, out uniqueMatchings, out prioritizedMen, out men1);
                
                //List<int[]> popularMatchings2;
                //List<int[]> uniqueMatchings2;
                //List<List<int[]>> prioritizedMen2;
                //List<List<int[]>> men12;
                //RunAlgorithm(women, men, out popularMatchings2, out uniqueMatchings2, out prioritizedMen2, out men12);
                //popularMatchings2 = popularMatchings2.Select(matching => InvertIntArray(matching)).ToList();
                //uniqueMatchings2 = uniqueMatchings2.Select(matching => InvertIntArray(matching)).ToList();

                //popularMatchings.AddRange(popularMatchings2);
                //uniqueMatchings.AddRange(uniqueMatchings2);

                int[][] overlap = Union(popularMatchings, uniqueMatchings).ToArray();

                var sizes = popularMatchings.Select(arr => NoNegativeEntries(arr)).ToArray();
                Array.Sort(sizes);
                if (!(sizes.Length > 1 && (sizes[0] != sizes[1])))
                {
                    continue;
                }

                if (!(overlap.Length != popularMatchings.Count))
                {
                    continue;
                }

                lock (Lock)
                {
                    Console.WriteLine("--------------------------------------------------------------------\n\n");

                    Console.WriteLine("int[][] men = new int[" + men.Length + "][]\n{");
                    bool first = true;
                    foreach (var man in men)
                    {
                        if (!first)
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

        static void Main2()
        {
            int[][] men = new int[8][]
             {  new int[7] { 0,2,7,3,4,1,5 },
                new int[7] { 1,2,4,3,0,7,5 },
                new int[7] { 7,4,0,3,5,1,2 },
                new int[5] { 2,1,5,7,4 },
                new int[8] { 6,1,4,0,2,5,7,3 },
                new int[7] { 0,5,4,7,3,1,2 },
                new int[2] { 6,4 },
                new int[7] { 2,7,3,4,1,5,0 }
             };
            int[][] women = new int[8][]
            {   new int[6] { 2,4,0,1,7,5 },
                new int[7] { 1,2,3,4,0,7,5 },
                new int[7] { 0,7,4,3,5,1,2 },
                new int[6] { 1,4,7,5,2,0 },
                new int[8] { 6,1,4,0,2,5,7,3 },
                new int[7] { 0,5,4,7,3,1,2 },
                new int[2] { 6,4 },
                new int[7] { 0,2,7,3,4,1,5 }
            };

            List<int[]> popularMatchings;
            List<int[]> uniqueMatchings;
            List<List<int[]>> prioritizedMen;
            List<List<int[]>> men1;
            RunAlgorithm(men, women, out popularMatchings, out uniqueMatchings, out prioritizedMen, out men1);

            //KavithaAlgorithm(men, women, new int[5] { 1,2,5,6,7});

            const string format = "{0,-32} :{1}";
            for (int i = 0; i < uniqueMatchings.Count; i++)
            {
                Console.Write("------------------------");
                Console.Write(CollectionToString(uniqueMatchings[i]));
                Console.WriteLine("------------------------");
                for (int j = 0; j < prioritizedMen[i].Count; j++)
                {
                    Console.WriteLine(format, CollectionToString(prioritizedMen[i][j]), CollectionToString(men1[i][j]));
                }
                Console.WriteLine();
            }

            Console.WriteLine("Men:");
            for (int i = 0; i < men.Length; i++)
            {
                Console.WriteLine(i + ":" + CollectionToString(men[i]));
            }
            Console.WriteLine("Women:");
            for (int i = 0; i < women.Length; i++)
            {
                Console.WriteLine(i + ":" + CollectionToString(women[i]));
            }

            Console.WriteLine();
            foreach (var matching in popularMatchings)
            {
                Console.WriteLine(CollectionToString(matching));
            }
        }

        static bool RUN_SEARCH = false;

        static void Main(string[] args)
        {
            if (RUN_SEARCH)
            {
                Random r = new Random(43262);
                Thread[] array = new Thread[6];
                for (int i = 0; i < array.Length; i++)
                {
                    // Start the thread with a ParameterizedThreadStart.
                    ParameterizedThreadStart start = new ParameterizedThreadStart(Main_Aux);
                    array[i] = new Thread(start);
                    array[i].Start(r.Next(100000));
                } 
            }
            else
            {
                Main2();
            }

            Console.Read();
        }
    }
}

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
        private static void AddToArray(int[] arr, int a)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] += a;
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
        




        //this algorithm generates a collection of matchings
        private static IEnumerable<int[]> BruteForceAlgorithm(int[][] men, int[][] women)
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

        private static IEnumerable<int[]> PopularMatching(int[][] men, int[][] women, IEnumerable<int[]> matchings)
        {
            List<int[]> output = new List<int[]>();
            int[][] matchingsArr = matchings.ToArray();
            for (int i = 0; i < matchingsArr.Length; i++)
            {
                bool popular = true;
                for (int j = 0; j < matchingsArr.Length; j++)
                {
                    if (i != j)
                    {
                        if (CompareMatching(men, women, matchingsArr[i], matchingsArr[j]) < 0)
                        {
                            popular = false;
                            break;
                        }
                    }
                }
                if (popular)
                {
                    output.Add(matchingsArr[i]);
                }
            }
            return output;
        }


        private static object Lock = new object();
        
        private static void RunAlgorithm(int[][] men, int[][] women, out List<int[]> popularMatchings, out List<List<int[]>> prioritizedMen, out IEnumerable<KeyValuePair<int[], List<int[]>>> matchingToMen1)
        {
            prioritizedMen = new List<List<int[]>>();

            //This is a map that maps a matching to a list of lists of men.
            Dictionary<int[], List<int[]>> matchingToMen1Dict = new Dictionary<int[], List<int[]>>(Matching.equalityComparer);

            foreach (var prioritizedMenI in Subset(men.Length))
            {
                KavithaAlgorithmOuptut kOut = KavithaAlgorithm(men, women, prioritizedMenI);

                if (matchingToMen1Dict.ContainsKey(kOut.matching))
                {
                    matchingToMen1Dict[kOut.matching].Add(kOut.men1);
                }
                else
                {
                    List<int[]> temp = new List<int[]>();
                    temp.Add(kOut.men1);
                    matchingToMen1Dict[kOut.matching] = temp;
                }
            }
            matchingToMen1 = matchingToMen1Dict;

            List<int[]> matchings = BruteForceAlgorithm(men, women).ToList();

            popularMatchings = PopularMatching(men, women, matchings).ToList();
        }

        static void Main_Aux(object seed)
        {
            foreach (var item in PreferenceListCombination(4, (int)seed))
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

                List<int[]> popularMatchings2;
                List<int[]> uniqueMatchings2;
                List<List<int[]>> prioritizedMen2;
                List<List<int[]>> men12;
                RunAlgorithm(women, men, out popularMatchings2, out uniqueMatchings2, out prioritizedMen2, out men12);
                popularMatchings2 = popularMatchings2.Select(matching => InvertIntArray(matching)).ToList();
                uniqueMatchings2 = uniqueMatchings2.Select(matching => InvertIntArray(matching)).ToList();

                popularMatchings.AddRange(popularMatchings2);
                uniqueMatchings.AddRange(uniqueMatchings2);

                int[][] overlap = popularMatchings.Union(uniqueMatchings, matchingEqualityComparer).ToArray();

                var sizes = popularMatchings.Select(arr => NonNegativeEntries(arr)).ToArray();
                Array.Sort(sizes);
                if (sizes[0] != sizes[1])
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

            List<int[]> popularMatchings;
            List<int[]> uniqueMatchings;
            List<List<int[]>> prioritizedMen;
            List<List<int[]>> men1;
            RunAlgorithm(men, women, out popularMatchings, out uniqueMatchings, out prioritizedMen, out men1);



            popularMatchings.Sort(nonNegativeEntriesCmp);
            List<int[]> stableMatchings = new List<int[]>();
            for (int i = 0; i < popularMatchings.Count; i++)
            {
                stableMatchings.Add(popularMatchings[i]);
                if (NonNegativeEntries(popularMatchings[i]) != NonNegativeEntries(popularMatchings[0]))
                {
                    break;
                }
            }



            KavithaAlgorithm(men, women, new int[5] { 1,2,5,6,7}, stableMatchings);

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

            Console.WriteLine();
            Console.WriteLine("Popular matchings:");
            foreach (var matching in popularMatchings)
            {
                int perfectCount = 0;
                for (int i = 0; i < matching.Length; i++)
                {
                    if (men[i].Length > 0)
                    {
                        if (matching[i] == men[i][0])
                        {
                            perfectCount++;
                        } 
                    }
                }
                for (int i = 0; i < women.Length; i++)
                {
                    if (women[i].Length > 0)
                    {
                        int index = -1;
                        for (int j = 0; j < matching.Length; j++)
                        {
                            if (matching[j] == i)
                            {
                                index = j;
                                break;
                            }
                        }
                        if (index >= 0)
                        {
                            if (index == women[i][0])
                            {
                                perfectCount++;
                            }
                        }
                    }
                }
                Console.WriteLine(format, CollectionToString(matching), perfectCount);
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

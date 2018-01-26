using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PopularMatching
{
    struct BeforeAfter
    {
        public IEnumerable<int> before;
        public IEnumerable<int> after;
    }

    struct PreferenceLists
    {
        public int[][] men;
        public int[][] women;
    }

    struct Matchings
    {
        public int n;
        public int min;
        public int max;
        public int[][] dominant;
        public int[][] middle;
        public int[][] stable;
    }

    struct Scenario
    {
        public bool ideal;
        public PreferenceLists preferenceLists;
        public Matchings matchings;
    }

    public static class Program
    {
        private static IEnumerable<PreferenceLists> RandomPreferenceListsByMen(int n, int maxManPrefListLength, int seed)
        {
            int[][] orderedSubsets = Utility.OrderedSubset(Enumerable.Range(0, n)).Select(ss => ss.ToArray()).Where(ss => ss.Length <= maxManPrefListLength).ToArray();

            Random r = new Random(seed);

            bool finished = false;
            while (!finished)
            {
                var men = Enumerable.Range(0, n).Select(i => orderedSubsets[r.Next(orderedSubsets.Length)]).ToArray();

                var women = Enumerable.Range(0, n).Select(i => new List<int>()).ToArray();

                int[] menOrder = Enumerable.Range(0, n).OrderBy(x => r.Next()).ToArray();

                for (int i = 0; i < n; i++)
                {
                    int manI = menOrder[i];
                    for (int j = 0; j < men[manI].Length; j++)
                    {
                        int womanI = men[manI][j];
                        women[womanI].Add(manI);
                    }
                }

                yield return new PreferenceLists()
                {
                    men = men,
                    women = women.Select(a => a.ToArray()).ToArray()
                };
            }
        }

        private static IEnumerable<int[][][]> RandomPreferenceLists(int n, int seed)
        {
            int[][] orderedSubsets = Utility.OrderedSubset(Enumerable.Range(0, n)).Select(ss => ss.ToArray()).ToArray();

            int[][][] output = new int[2][][];
            output[0] = new int[n][];
            output[1] = new int[n][];

            Random r = new Random(seed);
            
            int digitsTotal = 2 * n;
            bool finished = false;
            while (!finished)
            {
                for (int i = 0; i < digitsTotal; i++)
                {
                    int g = i / n;
                    output[g][i - (g * n)] = orderedSubsets[r.Next(orderedSubsets.Length)];
                }

                var men = output[0];
                var women = output[1];

                bool failed = false;
                for (int j = 0; j < men.Length; j++)
                {
                    var manPrefList = men[j];
                    for (int g = 0; g < manPrefList.Length; g++)
                    {
                        var womanPrefList = women[manPrefList[g]];
                        if (!womanPrefList.Contains(j))
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed)
                        break;
                }

                if (!failed)
                {
                    for (int j = 0; j < women.Length; j++)
                    {
                        var womanPrefList = women[j];
                        for (int g = 0; g < womanPrefList.Length; g++)
                        {
                            var manPrefList = men[womanPrefList[g]];
                            if (!manPrefList.Contains(j))
                            {
                                failed = true;
                                break;
                            }
                        }
                        if (failed)
                            break;
                    } 
                }

                if (!failed)
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

        //this algorithm generates all possible matchings
        private static IEnumerable<int[]> ValidMatchings(int[][] men, int[][] women)
        {
            var menSequence = Enumerable.Range(0, men.Length);

            //represents a three-dimensional jagged array
            //i0 = the number of unmatched men
            //i1 = possible set of unmatched men
            //i2 = unmatched man
            var unmatchedMen = Enumerable.Range(0, men.Length + 1).Select(unmatchedMenCount => {
                return Enumerable.Range(0, men.Length).Subset().Where(i => i.Count() == unmatchedMenCount).Select(i => i.ToArray()).ToArray();
            }).ToArray();
            
            foreach (var matchedWomen in Enumerable.Range(0, women.Length).OrderedSubset().Where(m => m.Count() <= men.Length))
            {
                int[] cpy = matchedWomen.ToArray();
                
                int unmatchedMenCount = men.Length - cpy.Length;
                int[][] possibleUnmatchedMen = unmatchedMen[unmatchedMenCount];
                for (int i = 0; i < possibleUnmatchedMen.Length; i++)
                {
                    int[] unmatchedMen2 = possibleUnmatchedMen[i];

                    int[] output = new int[men.Length];

                    int j = 0;
                    int k = 0;
                    for (int l = 0; l < output.Length; l++)
                    {
                        if (j < unmatchedMenCount && l == unmatchedMen2[j])
                        {
                            output[l] = -1;
                            j++;
                        }
                        else if(k < cpy.Length)
                        {
                            output[l] = cpy[k++];
                        }
                        else
                        {
                            throw new Exception("Not enough unmatched and matched men");
                        }
                    }

                    bool passes = true;
                    for (int l = 0; l < output.Length; l++)
                    {
                        int woman = output[l];
                        if (woman >= 0)
                        {
                            if (!(women[woman].Contains(l) && men[l].Contains(woman)))
                            {
                                passes = false;
                            }
                        }
                    }

                    if (passes)
                    {
                        yield return output;
                    }
                }
            }
        }

        //this algorithm generates a collection of matchings
        private static IEnumerable<int[]> BruteForceAlgorithm(int[][] men, int[][] women)
        {
            foreach (var matching in Utility.Permutation(Enumerable.Range(0, men.Length)))
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
        private static IEnumerable<int[]> PopularMatchings(this IEnumerable<int[]> matchings, int[][] men, int[][] women)
        {
            MatchingPopularityComparer comparer = new MatchingPopularityComparer(men, women);

            int[][] matchingsArray = matchings.ToArray();

            List<int[]> output = new List<int[]>();
            int[][] matchingsArr = matchings.ToArray();
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
                    output.Add(matchingsArr[i]);
                }
            }
            return output;

            //return matchingsArray.Where(matching => {
            //    return matchingsArray.All(curr => comparer.Compare(matching, curr) >= 0);
            //});
        }

        //private static IEnumerable<int[]> StableMatchings(this IEnumerable<int[]> popularMatchings)
        //{
        //    int[] min = popularMatchings.Aggregate((a, b) => (Matching.stableComparer.Compare(a, b) > 0) ? a : b);
        //    return popularMatchings.Where(popularMatching =>
        //    {
        //        return Matching.stableComparer.Compare(min, popularMatching) == 0;
        //    });
        //}

        static void FindPrefLists()
        {
            
            int[] emptyPrioritizedSet = new int[0] { };

            foreach (var prefLists in RandomPreferenceLists(6, 92874))
            {
                var m = prefLists[0];
                var w = prefLists[1];

                ContinuousKavitha.Output cO = ContinuousKavitha.Run(m, w, emptyPrioritizedSet);
                DiscreteKavitha.Output dO = DiscreteKavitha.Run(m, w, emptyPrioritizedSet);

                if (!MatchingEqualityComparer.INSTANCE.Equals(cO.matching, dO.matching))
                {
                    var promotedMen = cO.men1;

                    var nM = m.Where(i => true).ToArray();
                    var nW = w.Select(pl => pl.Where(manI => !promotedMen.Contains(manI)).ToArray()).ToArray();

                    foreach (var manI in promotedMen)
                    {
                        nM[manI] = emptyPrioritizedSet;
                    }

                    var promotedMenWomen = cO.matching.Where((wI, i) => promotedMen.Contains(i));

                    foreach (var womanI in promotedMenWomen)
                    {
                        nW[womanI] = emptyPrioritizedSet;
                    }

                    nM = nM.Select(pl => pl.Where(womanI => !promotedMenWomen.Contains(womanI)).ToArray()).ToArray();

                    var matching = DiscreteKavitha.GaleShapley(nW, nM);

                    var fMatching = Utility.InvertIntArray(matching);

                    bool failed = true;
                    for (int i = 0; i < 6; i++)
                    {
                        if (cO.men0.Contains(i))
                        {
                            if (fMatching[i] != cO.matching[i])
                            {
                                failed = false;
                                break;
                            }
                        }
                    }

                    if (!failed)
                    {

                        cO = ContinuousKavitha.Run(m, w, emptyPrioritizedSet);
                        dO = DiscreteKavitha.Run(m, w, emptyPrioritizedSet);

                        if (!MatchingEqualityComparer.INSTANCE.Equals(cO.matching, dO.matching))
                        {
                            Utility.WriteLine("----------------------------- " + cO.matching.DefaultString() + " -------------------------------");
                            Utility.WriteLine("----------------------------- " + dO.matching.DefaultString() + " -------------------------------");

                            Utility.WriteLine();
                            Utility.WriteLine(Utility.CollectionToString(m.Select((pl, i) => "\tnew int [" + pl.Count() + "] " + pl.DefaultString()), "int[][] men = new int [5][] {\n", ",\n", "\n};"));

                            Utility.WriteLine();
                            Utility.WriteLine(Utility.CollectionToString(w.Select((pl, i) => "\tnew int [" + pl.Count() + "] " + pl.DefaultString()), "int[][] women = new int [5][] {\n", ",\n", "\n};"));
                            Console.Read();
                        }

                    }

                }
            }
        }

        //static int[] UnmatchedMen(int[][] men, int[][] women, int[] c)
        //{
        //    int[] output = new int[men.Length];
        //    for (int i = 0; i < c.Length; i++)
        //    {
        //        output[c[i]] = -1;
        //    }
        //    for (int i = 0; i < c.Length; i++)
        //    {
        //        int manI = c[i];

        //        int[] manPrefList = men[manI];

        //        for (int j = 0; j < manPrefList.Length; j++)
        //        {
        //            int womanI = manPrefList[j];

        //            int[] womansPrefList = women[womanI];
        //            if (womansPrefList.Length > 0)
        //            {
        //                int mostPreferredMan = womansPrefList[0];
        //                if (c.Contains(mostPreferredMan))
        //                {
        //                    throw new Exception("Not possible");
        //                }
        //                else if (output[mostPreferredMan] == -1)
        //                {
        //                    output[mostPreferredMan] = womanI;
        //                }
        //                else
        //                {
        //                    int currentWomanOfMostPreferredMan = output[mostPreferredMan];
        //                    if (Array.IndexOf(manPrefList, currentWomanOfMostPreferredMan) > Array.IndexOf(manPrefList, womanI))
        //                    {
        //                        output[mostPreferredMan] = womanI;
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                throw new Exception("Preferences lists are not symmetric");
        //            }
        //        }
        //    }
        //}

        /**
         * Given two non-empty lists of matches this function will check if there exists a common element at the same index.
         **/
        static bool CommonAndIdenticalElement(int i, int[][] a, int[][] b, out int el)
        {
            int aEl;
            if(!CommonAndIdenticalElement(i, a, out aEl))
            {
                el = -1;
                return false;
            }
            int bEl;
            if (!CommonAndIdenticalElement(i, b, out bEl))
            {
                el = -1;
                return false;
            }

            if (aEl != bEl)
            {
                el = -1;
                return false;
            }
            else
            {
                el = aEl;
                return true;
            }
        }

        /**
         * Given a non-empty list of matches this function will check if there exists a common element at the same index.
         **/
        static bool CommonAndIdenticalElement(int i, int[][] arr, out int el)
        {
            el = arr[0][i];
            for (int j = 1; j < arr.Length; j++)
            {
                int next = arr[j][i];
                if (next != el)
                    return false;
            }
            return true;
        }

        static bool AnyElementDoesNotEqual(int i, int[][] arr, int el)
        {
            for (int j = 0; j < arr.Length; j++)
            {
                if (arr[j][i] != el)
                    return true;
            }
            return false;
        }

        static bool FilterFunction(Matchings matchings)
        {

            for (int manI = 0; manI < matchings.n; manI++)
            {
                int sharedWomanI;
                if(matchings.dominant.Length > 0 && matchings.stable.Length > 0)
                {
                    if (!CommonAndIdenticalElement(manI, matchings.dominant, matchings.stable, out sharedWomanI))
                        continue;
                }
                else if (matchings.stable.Length > 0)
                {
                    if (!CommonAndIdenticalElement(manI, matchings.stable, out sharedWomanI))
                        continue;
                }
                else if (matchings.dominant.Length > 0 )
                {
                    if (!CommonAndIdenticalElement(manI, matchings.dominant, out sharedWomanI))
                        continue;
                }
                else
                {
                    //there are no stable or dominant matchings
                    continue;
                }
                
                if(AnyElementDoesNotEqual(manI, matchings.middle, sharedWomanI))
                {
                    return true;
                }
            }

            return false;

        }

        static Matchings CreateMatchings(PreferenceLists preferenceLists)
        {
            var popularMatchings = ValidMatchings(preferenceLists.men, preferenceLists.women).PopularMatchings(preferenceLists.men, preferenceLists.women).ToArray();

            var unmatchedCounts = popularMatchings.Select(matching => matching.Where(m => m == -1).Count()).ToArray();
            int max = unmatchedCounts.Max();
            int min = unmatchedCounts.Min();

            return new Matchings()
            {
                n = preferenceLists.men.Length,
                min = min,
                max = max,
                dominant = popularMatchings.Where((m, i) => unmatchedCounts[i] == min).ToArray(),
                middle = popularMatchings.Where((m, i) => (unmatchedCounts[i] > min && unmatchedCounts[i] < max)).ToArray(),
                stable = popularMatchings.Where((m, i) => unmatchedCounts[i] == max).ToArray()
            };
        }

        static IEnumerable<Scenario> ValidScenarios(IEnumerable<PreferenceLists> prefLists, int threadCount)
        {
            //foreach (var preferenceLists in prefLists)
            //{
            //    var matchings = CreateMatchings(preferenceLists);

            //    yield return new Scenario()
            //    {
            //        ideal = FilterFunction(matchings),
            //        preferenceLists = preferenceLists,
            //        matchings = matchings
            //    };
            //}

            //yield break;

            BlockingCollection<PreferenceLists> input = new BlockingCollection<PreferenceLists>();
            BlockingCollection<Scenario> output = new BlockingCollection<Scenario>();

            var enumerator = prefLists.GetEnumerator();

            for (int i = 0; i < (threadCount * 2); i++)
            {
                enumerator.MoveNext();
                input.Add(enumerator.Current);
            }

            for (int threadI = 0; threadI < threadCount; threadI++)
            {
                Task.Factory.StartNew(() => {

                    foreach (var preferenceLists in input.GetConsumingEnumerable())
                    {
                        var matchings = CreateMatchings(preferenceLists);

                        output.Add(new Scenario()
                        {
                            ideal = FilterFunction(matchings),
                            preferenceLists = preferenceLists,
                            matchings = matchings
                        });

                    }

                });
            }
            
            foreach (var scenario in output.GetConsumingEnumerable())
            {
                enumerator.MoveNext();
                input.Add(enumerator.Current);

                if (scenario.ideal)
                {
                    yield return scenario;
                }
            }
        }

        static void Testing()
        {
            //foreach(var item in Utility.OrderedSubset(Enumerable.Range(0, 8)).Select(ss => ss.ToArray()).Where(ss => ss.Length <= 3))
            //{
            //    Utility.WriteLine(Utility.DefaultString(item));
            //}

            //Utility.WriteLine("done");

            int n = 8;

            var items = ValidScenarios(RandomPreferenceListsByMen(n, 2, 3211), 7);

            foreach (var item in items)
            {
                Utility.WriteLine("men:");
                Utility.WriteLine(Utility.NewLineString(item.preferenceLists.men.Select(Utility.DefaultString)));
                Utility.WriteLine();
                Utility.WriteLine("women:");
                Utility.WriteLine(Utility.NewLineString(item.preferenceLists.women.Select(Utility.DefaultString)));
                Utility.WriteLine();

                Utility.WriteLine("# of unmatched men: [{0}, {1}]", item.matchings.min, item.matchings.max);
                Utility.WriteLine();

                Utility.WriteLine("Matchings:");

                Utility.WriteLine("stable:");
                Utility.WriteLine(Utility.NewLineIndented(item.matchings.stable.Select(Utility.DefaultString)));
                Utility.WriteLine();

                Utility.WriteLine("middle:");
                Utility.WriteLine(Utility.NewLineIndented(item.matchings.middle.Select(Utility.DefaultString)));
                Utility.WriteLine();

                Utility.WriteLine("dominant:");
                Utility.WriteLine(Utility.NewLineIndented(item.matchings.dominant.Select(Utility.DefaultString)));
                Utility.WriteLine();

                //Utility.WriteLine(Utility.CollectionToString(item.Select(m => Utility.CollectionToString(m.Select(h => Utility.DefaultString(h)), "", "\n", "")), "", "\n\n\n\n", ""));
                Utility.WriteLine();
                //Utility.Write("Continue?(y/n)");
                //var cont = Console.ReadKey();
                //if (cont.Key != ConsoleKey.Y)
                //    break;
                Utility.WriteLine();
                Utility.consoleFileStream.Flush();
            }
            
        }


        static void Main(string[] args)
        {
            //FindPrefLists();

            Testing();
            return;

            int[][] men = new int[8][] {
                new int[1] { 0 },
                new int[2] { 0, 1 },
                new int[3] { 3, 1, 2 },
                new int[1] { 3 },
                new int[1] { 4 },
                new int[2] { 4, 5 },
                new int[3] { 7, 5, 6 },
                new int[1] { 7 }
            };

            int[][] women = new int[8][] {
                new int[2] { 1, 0 },
                new int[2] { 1, 2 },
                new int[1] { 2 },
                new int[2] { 2, 3 },
                new int[2] { 5, 4 },
                new int[2] { 5, 6 },
                new int[1] { 6 },
                new int[2] { 6, 7 }
            };


            //int[] c = new int[1]
            //{
            //    0
            //};

            //try
            //{
            //    int[] matching = UnmatchedMen(men, women, c);
            //}
            //catch (Exception e)
            //{
            //    Utility.WriteLine("Exception: " + e.Message);
            //}








            const bool USE_DISCRETE = true;
            
            var kavithaOutputs = new List<int[]>();
            var gKavithaOutputs = new List<int[]>();


            //Run Continuous Kavitha's Algorithm
            var cResults = new Dictionary<int[], List<BeforeAfter>>(MatchingEqualityComparer.INSTANCE);
            foreach (var prioritizedMen in Enumerable.Range(0, men.Length).Subset())
            {
                ContinuousKavitha.Output o = ContinuousKavitha.Run(men, women, prioritizedMen);

                var result = new BeforeAfter()
                {
                    before = prioritizedMen,
                    after = o.men1
                };

                if (cResults.ContainsKey(o.matching))
                {
                    cResults[o.matching].Add(result);
                }
                else
                {
                    List<BeforeAfter> temp = new List<BeforeAfter>();
                    temp.Add(result);
                    cResults.Add(o.matching, temp);
                }
            }

            //Run Discrete Kavitha's Algorithm
            var dResults = new Dictionary<int[], List<BeforeAfter>>(MatchingEqualityComparer.INSTANCE);
            foreach (var prioritizedMen in Enumerable.Range(0, men.Length).Subset())
            {
                DiscreteKavitha.Output o = DiscreteKavitha.Run(men, women, prioritizedMen);

                var result = new BeforeAfter() {
                    before = prioritizedMen,
                    after = o.men1
                };

                if (dResults.ContainsKey(o.matching))
                {
                    dResults[o.matching].Add(result);
                }
                else
                {
                    List<BeforeAfter> temp = new List<BeforeAfter>();
                    temp.Add(result);
                    dResults.Add(o.matching, temp);
                }
            }


            Utility.WriteLine();
            Utility.WriteLine();
            Utility.WriteLine("Continuous Kavitha Algorithm:");

            const string format = "{0,-32} :{1}";
            foreach (var kvPair in cResults)
            {
                Utility.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                Utility.WriteLine(Utility.CollectionToString(kvPair.Value.Select((result) =>
                {
                    return string.Format(format, result.before.DefaultString(), result.after.DefaultString());
                }), "", "\n", ""));
            }

            Utility.WriteLine();
            Utility.WriteLine();
            Utility.WriteLine();
            Utility.WriteLine("Discrete Kavitha Algorithm:");
            foreach (var kvPair in dResults)
            {
                Utility.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                Utility.WriteLine(Utility.CollectionToString(kvPair.Value.Select((result) =>
                {
                    return string.Format(format, result.before.DefaultString(), result.after.DefaultString());
                }), "", "\n", ""));
            }
            
            Utility.WriteLine();
            Utility.WriteLine("Unique outputs:");
            Utility.WriteLine();
            Utility.WriteLine("Continuous Kavitha Algorithm:");
            Utility.WriteLine(Utility.CollectionToString(cResults.Keys.Select(key => key.DefaultString()), "", "\n", ""));
            Utility.WriteLine();
            Utility.WriteLine("Discrete Kavitha Algorithm:");
            Utility.WriteLine(Utility.CollectionToString(dResults.Keys.Select(key => key.DefaultString()), "", "\n", ""));
            
            Utility.WriteLine();
            Utility.WriteLine("Brute force popular matchings:");
            Utility.WriteLine(Utility.CollectionToString(ValidMatchings(men, women).PopularMatchings(men, women).Select(m => m.DefaultString()), "", "\n", ""));


            Utility.WriteLine();
            Utility.WriteLine("men:");
            Utility.WriteLine(Utility.CollectionToString(men.Select((pl, i) => i + ": " + pl.DefaultString()), "", "\n", ""));

            Utility.WriteLine();
            Utility.WriteLine("women:");
            Utility.WriteLine(Utility.CollectionToString(women.Select((pl, i) => i + ": " + pl.DefaultString()), "", "\n", ""));

            //Utility.WriteLine();
            //Utility.WriteLine("popularMatchings:");
            //var popularMatchings = ValidMatchings(men, women).Distinct(MatchingEqualityComparer.INSTANCE).PopularMatchings(men, women);
            //foreach (var popularMatching in popularMatchings)
            //{
            //    Utility.WriteLine(popularMatching.DefaultString());
            //}

            //Utility.WriteLine();
            //Console.Write("Filtering duplicate outputs ");
            //kavithaOutputs = kavithaOutputs.Distinct().ToList();
            //gKavithaOutputs = gKavithaOutputs.Distinct().ToList();
            //Utility.WriteLine("done");

            //Utility.WriteLine();
            //Utility.WriteLine("original:");
            //Utility.WriteLine(Utility.CollectionToString(kavithaOutputs.Select(list => list.DefaultString()), "", "\n", ""));
            //Utility.WriteLine();
            //Utility.WriteLine("general:");
            //Utility.WriteLine(Utility.CollectionToString(gKavithaOutputs.Select(list => list.DefaultString()), "", "\n", ""));

            //Utility.WriteLine();
            //bool popSubsetOfOriginal = popularMatchings.All(a => kavithaOutputs.Contains(a, MatchingEqualityComparer.INSTANCE));
            //Utility.WriteLine("(popular matchings) ⊆(original Kavitha algorithm outputs)? " + ((popSubsetOfOriginal) ? "YES" : "NO"));
            //Utility.WriteLine();

            //bool popSubsetOfGenKavitha = popularMatchings.All(a => gKavithaOutputs.Contains(a, MatchingEqualityComparer.INSTANCE));
            //Utility.WriteLine("(popular matchings) ⊆(general Kavitha algorithm outputs)? " + ((popSubsetOfGenKavitha) ? "YES" : "NO"));
            //Utility.WriteLine();

            Console.Read();
        }
    }
}

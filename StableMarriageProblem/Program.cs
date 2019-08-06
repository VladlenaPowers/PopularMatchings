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

    public static class Program
    {
        private static IEnumerable<PreferenceLists> RandomPreferenceListsByMen(int n, int maxManPrefListLength, int seed)
        {
            int[][] orderedSubsets = Utility.OrderedSubset(Enumerable.Range(0, n)).Select(ss => ss.ToArray()).Where(ss => ss.Length <= maxManPrefListLength).ToArray();

            Random r = new Random(seed);

            bool finished = false;
            while (!finished)
            {
                var men = new int[n][];
                var women = new List<int>[n];

                for (int i = 0; i < n; i++)
                {
                    men[i] = orderedSubsets[r.Next(orderedSubsets.Length)];
                    women[i] = new List<int>(n);
                }

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

                yield return new PreferenceLists(men, women.Select(a => a.ToArray()).ToArray());
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
        public static IEnumerable<int[]> ValidMatchings(int[][] men, int[][] women)
        {
            var menSequence = Enumerable.Range(0, men.Length);

            //represents a three-dimensional jagged array
            //i0 = the number of unmatched men
            //i1 = possible set of unmatched men~
            //i2 = unmatched man
            var unmatchedMen = Enumerable.Range(0, men.Length + 1).Select(unmatchedMenCount => {
                return Enumerable.Range(0, men.Length).Subset().Where(i => i.Count() == unmatchedMenCount).Select(i => i.ToArray()).ToArray();
            }).ToArray();

            var matchedWomenSets = Enumerable.Range(0, women.Length).OrderedSubset().Where(m => m.Count() /*<=*/<= men.Length);

            foreach (var matchedWomen in matchedWomenSets)
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
        public static IEnumerable<int[]> ParetoOptimalMatchings(this IEnumerable<int[]> matchings, int[][] men, int[][] women)
        {
            ParetoOptimalityComparer comparer = new ParetoOptimalityComparer(men, women);

            int[][] matchingsArray = matchings.ToArray();

            List<int[]> output = new List<int[]>();
            int[][] matchingsArr = matchings.ToArray();
            for (int i = 0; i < matchingsArr.Length; i++)
            {
                bool paretoOptimal = true;
                for (int j = 0; j < matchingsArr.Length; j++)
                {
                    if (i != j)
                    {
                        if (comparer.Compare(matchingsArr[i], matchingsArr[j]) < 0)
                        {
                            paretoOptimal = false;
                            break;
                        }
                    }
                }
                if (paretoOptimal)
                {
                    output.Add(matchingsArr[i]);
                }
            }
            return output;

            //return matchingsArray.Where(matching => {
            //    return matchingsArray.All(curr => comparer.Compare(matching, curr) >= 0);
            //});
        }

        //returns all of the popular matchings for a given set of matchings
        public static IEnumerable<int[]> PopularMatchings(this IEnumerable<int[]> matchings, int[][] men, int[][] women)
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
        
        public static int MaxIntersections(this IEnumerable<int[]> matchings, int[] operand)
        {
            return matchings.Select(i => MatchingEqualityComparerByEdges.MatchingEdges(operand, i)).Max();
        }
        
        private static void DoTheThingCleanest()
        {
            const int n = 5;
            var randomPrefLists = RandomPreferenceLists(n, 53);

            var passedPrefLists = randomPrefLists
                .Select(pl => new PreferenceLists(pl[0], pl[1]))
                .Where(pl => pl.StableMatchings.Count() > 1)
                //.Where(pl => pl.ParetoOptimalMatchings.Count() == 2)
                //.Where(pl => pl.ParetoOptimalMatchings.Contains(pl.StableMatchings[0], MatchingEqualityComparer.INSTANCE))
                //.Where(pl => MaxIntersections(pl.ParetoOptimalMatchings, pl.GaleShapelyMatching) == 3)
                .Where(pl =>
                {
                    var stableMatchingsWithoutGS = pl.StableMatchings
                        .Where(m => !MatchingEqualityComparer.INSTANCE.Equals(pl.GaleShapelyMatching, m))
                        .ToArray();

                    var stableMaxIntersections = stableMatchingsWithoutGS
                        .Select(k => pl.ParetoOptimalMatchings.MaxIntersections(k))
                        .ToArray();

                    int maxValue = stableMaxIntersections.Max();
                    int gsMaxValue = pl.ParetoOptimalMatchings.MaxIntersections(pl.GaleShapelyMatching);

                    return gsMaxValue == maxValue - 2;
                })
                .Select(x => x);

            foreach (var prefLists in passedPrefLists)
            {
                Utility.WriteLine("----------------------------- Pref list found -------------------------------");

                Utility.WriteLine();
                Utility.WriteLine(Utility.CollectionToString(prefLists.men.Select((pl, i) => "\tnew int [" + (pl.Count()) + "] " + pl.DefaultString()), "int[][] men = new int [" + n + "][] {\n", ",\n", "\n};"));

                Utility.WriteLine();
                Utility.WriteLine(Utility.CollectionToString(prefLists.women.Select((pl, i) => "\tnew int [" + (pl.Count()) + "] " + pl.DefaultString()), "int[][] women = new int [" + n + "][] {\n", ",\n", "\n};"));
                Console.Read();
            }
        }

        private static void DoTheThingCleaner()
        {
            const int n = 4;
            foreach (var prefLists in RandomPreferenceLists(n, 35))
            {
                var m = prefLists[0];
                var w = prefLists[1];
                var pl = new PreferenceLists(m, w);

                //if (!m.All(pl => pl.Any()) || !w.All(pl => pl.Any()))
                //    continue;

                //var dominantCandidates = pl.PopularMatchings.Where((k, i) => unmatchedCounts[i] == min).ToArray();

                //List<int[]> dominant = new List<int[]>();
                //List<int[]> maxSizeNonDominant = new List<int[]>();
                //var popularityComparer = new MatchingPopularityComparer(m, w);

                //foreach (var candidate in dominantCandidates)
                //{
                //    var unmatchedMen = candidate.Where(x => x == -1).Count();

                //    var isMaxSizeNonDominant = ValidMatchings(m, w).Any(validMatching => {

                //        var result = popularityComparer.Compare(candidate, validMatching);

                //        if (result == 0)
                //        {
                //            var otherUnmatched = validMatching.Where(x => x == -1).Count();

                //            if (unmatchedMen > otherUnmatched)
                //            {
                //                maxSizeNonDominant.Add(candidate);
                //                return true;
                //            }
                //        }

                //        return false;
                //    });

                //    if (!isMaxSizeNonDominant)
                //    {
                //        dominant.Add(candidate);
                //    }
                //}

                // if (pl.MinSizeMatchings.Length < 2)
                //    continue;

                //if (middleMatchings.Length < 1)
                //    continue;

                //if (dominant.Count != 1)
                //    continue;

                var stableMaxIntersections = pl.StableMatchings
                    .Select(k => pl.ParetoOptimalMatchings.MaxIntersections(k))
                    .ToArray();

                int galeShapleyMaxIntersections = pl.ParetoOptimalMatchings
                    .MaxIntersections(pl.GaleShapelyMatching);

                if (pl.StableMatchings.Length != 1)
                    continue;

                //if (pl.ParetoOptimalMatchings.Length != 1)
                //    continue;

                //var cond0 = !pl.ParetoOptimalMatchings.Contains(pl.GaleShapelyMatching, mCmp);

                //if (!cond0)
                //    continue;

                //var cond1 = pl.StableMatchings.Any(k => !mCmp.Equals(pl.GaleShapelyMatching, k) && pl.ParetoOptimalMatchings.Contains(k, mCmp));

                //if (!cond1)
                //    continue;

                //int galeShapelyIndex = -1;
                //for (int i = 0; i < pl.StableMatchings.Length; i++)
                //{
                //    if (MatchingEqualityComparer.INSTANCE.Equals(pl.GaleShapelyMatching, pl.StableMatchings[i]))
                //    {
                //        galeShapelyIndex = i;
                //        break;
                //    }
                //}

                //if (galeShapelyIndex < 0)
                //    throw new Exception("Gale shapely not found in stable set");

                int maxDiff = 0;
                int maxValue = stableMaxIntersections.Max();

                //if ((galeshapleymaxintersections > 3))
                //    continue;

                if ((galeShapleyMaxIntersections != 3))
                    continue;

                if ((maxValue + 1 != galeShapleyMaxIntersections))
                    continue;

                //var maxMatching = pl.StableMatchings[Array.IndexOf(stableMaxIntersections, maxValue)];

                //if (!pl.ParetoOptimalMatchings.Contains(maxMatching, MatchingEqualityComparer.INSTANCE))
                //    continue;

                //if (pl.ParetoOptimalMatchings.Contains(DiscreteKavitha.GaleShapley(m, w), MatchingEqualityComparer.INSTANCE))
                //    continue;

                //if (pl.ParetoOptimalMatchings.Length > 1)
                //    continue;

                //bool skip = false;
                //foreach (var popMatch in pl.PopularMatchings)
                //{
                //    foreach (var paretoMatch in pl.ParetoOptimalMatchings)
                //    {
                //        if (!MatchingEqualityComparerByEdges.INSTANCE.Equals(popMatch, paretoMatch))
                //        {
                //            skip = true;
                //            break;
                //        }
                //    }

                //    if (skip)
                //        break;
                //}

                Utility.WriteLine("----------------------------- Pref list found -------------------------------");

                Utility.WriteLine();
                Utility.WriteLine(Utility.CollectionToString(m.Select((oPl, i) => "\tnew int [" + (oPl.Count()) + "] " + oPl.DefaultString()), "int[][] men = new int [" + n + "][] {\n", ",\n", "\n};"));

                Utility.WriteLine();
                Utility.WriteLine(Utility.CollectionToString(w.Select((oPl, i) => "\tnew int [" + (oPl.Count()) + "] " + oPl.DefaultString()), "int[][] women = new int [" + n + "][] {\n", ",\n", "\n};"));
                Console.Read();
            }
        }

        private static void DoTheThing()
        {
            const int n = 4;
            foreach (var prefLists in RandomPreferenceLists(n, 35))
            {
                var m = prefLists[0];
                var w = prefLists[1];


                var mCmp = new MatchingEqualityComparer();

                //if (!m.All(pl => pl.Any()) || !w.All(pl => pl.Any()))
                //    continue;

                var popularMatchings = ValidMatchings(m, w).PopularMatchings(m, w);//.ToArray();

                var unmatchedCounts = popularMatchings.Select(matching => matching.Where(k => k == -1).Count()).ToArray();
                int max = unmatchedCounts.Max();
                int min = unmatchedCounts.Min();
                var middleMatchings = popularMatchings.Where((k, i) => (unmatchedCounts[i] > min && unmatchedCounts[i] < max)).ToArray();
                var minSize = popularMatchings.Where((k, i) => unmatchedCounts[i] == max).ToArray();

                var dominantCandidates = popularMatchings.Where((k, i) => unmatchedCounts[i] == min).ToArray();

                //List<int[]> dominant = new List<int[]>();
                //List<int[]> maxSizeNonDominant = new List<int[]>();
                //var popularityComparer = new MatchingPopularityComparer(m, w);

                //foreach (var candidate in dominantCandidates)
                //{
                //    var unmatchedMen = candidate.Where(x => x == -1).Count();

                //    var isMaxSizeNonDominant = ValidMatchings(m, w).Any(validMatching => {

                //        var result = popularityComparer.Compare(candidate, validMatching);

                //        if (result == 0)
                //        {
                //            var otherUnmatched = validMatching.Where(x => x == -1).Count();

                //            if (unmatchedMen > otherUnmatched)
                //            {
                //                maxSizeNonDominant.Add(candidate);
                //                return true;
                //            }
                //        }

                //        return false;
                //    });

                //    if (!isMaxSizeNonDominant)
                //    {
                //        dominant.Add(candidate);
                //    }
                //}

               // if (minSize.Length < 2)
                //    continue;

                //if (middleMatchings.Length < 1)
                //    continue;

                //if (dominant.Count != 1)
                //    continue;




                var paretoOptimalMatchings = ValidMatchings(m, w).ParetoOptimalMatchings(m, w).ToArray();

                var galeShapleyMatching = DiscreteKavitha.GaleShapley(m, w);

                var edgeFinder = Program.PlusPlusEdgeFinder(new PreferenceLists(m, w));
                var stable = minSize
                    .Where(k => edgeFinder(k).Count() == 0)
                    .Where(k => !MatchingEqualityComparer.INSTANCE.Equals(k, galeShapleyMatching))
                    .ToArray();
                var stableMaxIntersections = stable.Select(k =>
                {
                    return paretoOptimalMatchings.Select(i => MatchingEqualityComparerByEdges.MatchingEdges(k, i)).Max();
                }).ToArray();

                int galeShapleyMaxIntersections = paretoOptimalMatchings.Select(i => MatchingEqualityComparerByEdges.MatchingEdges(galeShapleyMatching, i)).Max();

                if (stable.Length != 1)
                    continue;

                //if (paretoOptimalMatchings.Length != 1)
                //    continue;

                //var cond0 = !paretoOptimalMatchings.Contains(galeShapleyMatching, mCmp);

                //if (!cond0)
                //    continue;

                //var cond1 = stable.Any(k => !mCmp.Equals(galeShapleyMatching, k) && paretoOptimalMatchings.Contains(k, mCmp));

                //if (!cond1)
                //    continue;

                //int galeShapelyIndex = -1;
                //for (int i = 0; i < stable.Length; i++)
                //{
                //    if (MatchingEqualityComparer.INSTANCE.Equals(galeShapleyMatching, stable[i]))
                //    {
                //        galeShapelyIndex = i;
                //        break;
                //    }
                //}

                //if (galeShapelyIndex < 0)
                //    throw new Exception("Gale shapely not found in stable set");

                int maxDiff = 0;
                int maxValue = stableMaxIntersections.Max();

                //if ((galeshapleymaxintersections > 3))
                //    continue;

                if ((galeShapleyMaxIntersections != 3))
                    continue;

                if ((maxValue + 1 != galeShapleyMaxIntersections))
                    continue;

                //var maxMatching = stable[Array.IndexOf(stableMaxIntersections, maxValue)];

                //if (!paretoOptimalMatchings.Contains(maxMatching, MatchingEqualityComparer.INSTANCE))
                //    continue;





                //if (paretoOptimalMatchings.Contains(DiscreteKavitha.GaleShapley(m, w), MatchingEqualityComparer.INSTANCE))
                //    continue;

                //if (paretoOptimalMatchings.Length > 1)
                //    continue;

                //bool skip = false;
                //foreach (var popMatch in popularMatchings)
                //{
                //    foreach (var paretoMatch in paretoOptimalMatchings)
                //    {
                //        if (!MatchingEqualityComparerByEdges.INSTANCE.Equals(popMatch, paretoMatch))
                //        {
                //            skip = true;
                //            break;
                //        }
                //    }

                //    if (skip)
                //        break;
                //}

                if (true)
                {
                    Utility.WriteLine("----------------------------- Pref list found -------------------------------");

                    Utility.WriteLine();
                    Utility.WriteLine(Utility.CollectionToString(m.Select((pl, i) => "\tnew int [" + (pl.Count()) + "] " + pl.DefaultString()), "int[][] men = new int [" + n + "][] {\n", ",\n", "\n};"));

                    Utility.WriteLine();
                    Utility.WriteLine(Utility.CollectionToString(w.Select((pl, i) => "\tnew int [" +( pl.Count()) + "] " + pl.DefaultString()), "int[][] women = new int [" + n + "][] {\n", ",\n", "\n};"));
                    Console.Read();
                }
            }
        }

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
        static bool CommonElement(int index, int[][] a, int[][] b, out int el)
        {
            int aEl;
            if(!CommonElement(index, a, out aEl))
            {
                el = -1;
                return false;
            }
            int bEl;
            if (!CommonElement(index, b, out bEl))
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
        static bool CommonElement(int index, int[][] arr, out int el)
        {
            el = arr[0][index];
            for (int j = 1; j < arr.Length; j++)
            {
                int next = arr[j][index];
                if (next != el)
                    return false;
            }
            return true;
        }
        
        static bool FilterFunction(Matchings matchings)
        {
            if ((matchings.middle.Length == 0) || (matchings.minSize.Length == 0) || (matchings.dominant.Length == 0))
            {
                return false;
            }

            return true;

            for (int manI = 0; manI < matchings.n; manI++)
            {
                int sharedWomanI;
                if (!CommonElement(manI, matchings.dominant, matchings.minSize, out sharedWomanI))
                    continue;

                for (int j = 0; j < matchings.middle.Length; j++)
                {
                    if (matchings.middle[j][manI] != sharedWomanI)
                        return true;
                }
            }

            return false;
        }

        public static Func<int[], int[][]> PlusPlusEdgeFinder(PreferenceLists preferenceLists)
        {
            return (int[] matching) =>
            {
                int[] womenMatching = new int[preferenceLists.women.Length];
                for (int i = 0; i < womenMatching.Length; i++)
                    womenMatching[i] = -1;
                for (int i = 0; i < matching.Length; i++)
                {
                    var woman = matching[i];
                    if (woman >= 0)
                        womenMatching[woman] = i;
                }

                List<int[]> edges = new List<int[]>();

                for(int i = 0; i < matching.Length; i++)
                {
                    int matchedWoman = matching[i];
                    
                    for(int j = 0; j < preferenceLists.men[i].Length; j++)
                    {
                        int preferredWoman = preferenceLists.men[i][j];

                        if (preferredWoman == matchedWoman)
                            break;

                        var womanPrefList = preferenceLists.women[preferredWoman];

                        int womanMatchedMan = womenMatching[preferredWoman];

                        if (womanMatchedMan < 0)
                            break;
                        else
                        {
                            int matchedRank = 0;
                            while (matchedRank < womanPrefList.Length && womanPrefList[matchedRank] != womanMatchedMan)
                                matchedRank++;

                            int currManRank = 0;
                            while (currManRank < womanPrefList.Length && womanPrefList[currManRank] != i)
                                currManRank++;

                            if (currManRank < matchedRank)
                            {
                                edges.Add(new int[2] { i, preferredWoman });
                            }
                        }
                    }
                }

                return edges.ToArray();
            };
        }

        static Matchings CreateMatchings(PreferenceLists preferenceLists)
        {
            var unmatchedCounts = preferenceLists.PopularMatchings
                .Select(matching => matching.Where(m => m == -1).Count()).ToArray();
            int max = unmatchedCounts.Max();
            int min = unmatchedCounts.Min();

            var dominantCandidates = preferenceLists.PopularMatchings
                .Where((m, i) => unmatchedCounts[i] == min).ToArray();

            List<int[]> dominant = new List<int[]>();
            List<int[]> maxSizeNonDominant = new List<int[]>();
            var popularityComparer = new MatchingPopularityComparer(preferenceLists.men, preferenceLists.women);

            foreach (var candidate in dominantCandidates)
            {
                var unmatchedMen = candidate.Where(x => x == -1).Count();

                var isMaxSizeNonDominant = ValidMatchings(preferenceLists.men, preferenceLists.women).Any(validMatching => {

                    var result = popularityComparer.Compare(candidate, validMatching);

                    if (result == 0)
                    {
                        var otherUnmatched = validMatching.Where(x => x == -1).Count();

                        if (unmatchedMen > otherUnmatched)
                        {
                            maxSizeNonDominant.Add(candidate);
                            return true;
                        }
                    }

                    return false;
                });

                if(!isMaxSizeNonDominant)
                {
                    dominant.Add(candidate);
                }
            }

            return new Matchings()
            {
                n = preferenceLists.men.Length,
                min = min,
                max = max,
                dominant = dominant.ToArray(),
                dominantPlusPlusEdges = dominant.Select(PlusPlusEdgeFinder(preferenceLists)).ToArray(),
                middle = preferenceLists.PopularMatchings.Where((m, i) => (unmatchedCounts[i] > min && unmatchedCounts[i] < max)).ToArray(),
                minSize = preferenceLists.PopularMatchings.Where((m, i) => unmatchedCounts[i] == max).ToArray(),
                maxSizeNonDominant = maxSizeNonDominant.ToArray(),
                paretoOptimalMatchings = preferenceLists.ParetoOptimalMatchings
            };
        }

        static IEnumerable<Scenario> ValidScenarios(IEnumerable<PreferenceLists> prefLists, int threadCount)
        {
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
                yield return scenario;

                //if (scenario.ideal && scenario.matchings.maxSizeNonDominant.Length > 0)
                //{
                //    yield return scenario;
                //}
            }
        }

        static void Testing()
        {
            int n = 7;

            var items = ValidScenarios(RandomPreferenceListsByMen(n, 4, 273247), 8);

            foreach (var item in items)
            {
                Utility.WriteLine(item.ToString());
                Utility.WriteLine();
                Utility.consoleFileStream.Flush();
            }
            
        }

        static BitArray getActiveEdges(int[][] menPrefList, int[] matching)
        {
            int length = menPrefList.Select(l => l.Length).Sum() + 1;
            BitArray output = new BitArray(length);
            output[0] = true;

            int x = 1;
            for (int i = 0; i < menPrefList.Length; i++)
            {
                for (int j = 0; j < menPrefList[i].Length; j++)
                {
                    output[x++] = matching[i] == menPrefList[i][j];
                }
            }

            return output;
        }

        public static string makePolytopeExp(int[][] menPrefList, IEnumerable<int[]> matchings)
        {
            var arrStrings = matchings
                .Select(m => getActiveEdges(menPrefList, m))
                .Select(ba => Enumerable.Range(0, ba.Length).Select(i => ba[i] ? 1 : 0).ToArray())
                .Select(a => Utility.CollectionToString(a, "[", ",", "]"));

            return "new Polytope(POINTS=>" + Utility.CollectionToString(arrStrings, "[", ",", "]") + ")";
        }

        static void Main(string[] args)
        {
           DoTheThingCleanest();  return;

            //6FindPrefLists();
            //Testing();
            //return;

            int[][] men = new int[8][] {
        new int [4] { 2, 3, 0, 1},
        new int [5] { 0, 5, 1, 3, 2},
        new int [5] { 0, 2, 7, 3, 1},
        new int [4] { 3, 2, 1, 0},
        new int [4] { 6, 7, 4, 5},
        new int [5] { 4, 1, 5, 7, 6},
        new int [5] { 4, 6, 3, 7, 5},
        new int [4] { 7, 6, 5, 4}
};

            int[][] women = new int[8][] {
        new int [4] { 0, 1, 2, 3},
        new int [5] { 3, 1, 0, 2, 5},
        new int [4] { 1, 3, 0, 2},
        new int [5] { 2, 1, 3, 0, 6},
        new int [4] { 4, 5, 6, 7},
        new int [5] { 7, 5, 4, 6, 1},
        new int [4] { 5, 7, 4, 6},
        new int [5] { 6, 5, 7, 4, 2}
};





























            //            int[][] men = new int[9][] {
            //        new int [4] { 2, 3, 6, 0},
            //        new int [4] { 0, 5, 8, 2},
            //        new int [2] { 2, 0},
            //        new int [4] { 5, 6, 0, 3},
            //        new int [4] { 3, 8, 2, 5},
            //        new int [2] { 5, 3},
            //        new int [4] { 8, 0, 3, 6},
            //        new int [4] { 6, 2, 5, 8},
            //        new int [2] { 8, 6}
            //};

            //            int[][] women = new int[9][] {
            //        new int [5] { 0, 1, 2, 3, 6},
            //        new int [0] {},
            //        new int [5] { 1, 2, 0, 4, 7},
            //        new int [5] { 3, 4, 5, 6, 0},
            //        new int [0] {},
            //        new int [5] { 4, 5, 3, 7, 1},
            //        new int [5] { 6, 7, 8, 0, 3},
            //        new int [0] {},
            //        new int [5] { 7, 8, 6, 1, 4}
            //};
            //        int[][] men = new int[4][] {
            //        new int [3] { 2, 3, 1},
            //        new int [2] { 0, 3},
            //        new int [4] { 0, 1, 3, 2},
            //        new int [4] { 1, 2, 3, 0}
            //};

            //        int[][] women = new int[4][] {
            //        new int [3] { 3, 1, 2},
            //        new int [3] { 3, 2, 0},
            //        new int [3] { 3, 2, 0},
            //        new int [4] { 1, 2, 3, 0}

            //        int[][] men = new int[4][] {
            //        new int [4] { 0, 3, 2, 1},
            //        new int [4] { 2, 3, 0, 1},
            //        new int [4] { 3, 2, 1, 0},
            //        new int [4] { 2, 1, 0, 3}
            //};

            //        int[][] women = new int[4][] {
            //        new int [4] { 1, 0, 2, 3},
            //        new int [4] { 0, 3, 1, 2},
            //        new int [4] { 2, 3, 0, 1},
            //        new int [4] { 3, 0, 2, 1}
            //};














            PreferenceLists pl2 = new PreferenceLists(men, women);

            Matchings matchings = CreateMatchings(pl2);

            Scenario s = new Scenario()
            {
                matchings = matchings,
                preferenceLists = pl2,
                ideal = true
            };

            Console.WriteLine(s.ToString());

            Console.Read();

            return;


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

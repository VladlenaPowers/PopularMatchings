using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
    public class PreferenceLists
    {
        public readonly int[][] men;
        public readonly int[][] women;

        public readonly Lazy<int[]> _galeShapelyMatching;
        public readonly Lazy<int[][]> _validMatchings;
        public readonly Lazy<int[][]> _popularMatchings;
        public readonly Lazy<int[][]> _minSizeMatchings;
        public readonly Lazy<int[][]> _stableMatchings;
        public readonly Lazy<int[][]> _paretoOptimalMatchings;

        public int[] GaleShapelyMatching { get { return _galeShapelyMatching.Value; } }
        public int[][] ValidMatchings { get { return _validMatchings.Value; } }
        public int[][] PopularMatchings { get { return _popularMatchings.Value; } }
        public int[][] MinSizeMatchings { get { return _minSizeMatchings.Value; } }
        public int[][] StableMatchings { get { return _stableMatchings.Value; } }
        public int[][] ParetoOptimalMatchings { get { return _paretoOptimalMatchings.Value; } }

        public PreferenceLists(int[][] men, int[][] women)
        {
            this.men = men;
            this.women = women;

            _galeShapelyMatching = new Lazy<int[]>(() =>
            {
                return DiscreteKavitha.GaleShapley(men, women);
            });

            _validMatchings = new Lazy<int[][]>(() =>
            {
                return Program.ValidMatchings(men, women)
                    .ToArray();
            });

            _popularMatchings = new Lazy<int[][]>(() =>
            {
                return ValidMatchings
                    .PopularMatchings(men, women)
                    .ToArray();
            });

            _minSizeMatchings = new Lazy<int[][]>(() =>
            {
                var unmatchedCounts = PopularMatchings.Select(matching => matching.Where(k => k == -1).Count()).ToArray();
                int max = unmatchedCounts.Max();
                int min = unmatchedCounts.Min();
                var middleMatchings = PopularMatchings.Where((k, i) => (unmatchedCounts[i] > min && unmatchedCounts[i] < max)).ToArray();
                return PopularMatchings
                    .Where((k, i) => unmatchedCounts[i] == max)
                    .ToArray();
            });

            _stableMatchings = new Lazy<int[][]>(() =>
            {
                var edgeFinder = Program.PlusPlusEdgeFinder(this);
                return MinSizeMatchings
                    .Where(k => edgeFinder(k).Count() == 0)
                    .ToArray();
            });

            _paretoOptimalMatchings = new Lazy<int[][]>(() =>
            {
                return ValidMatchings
                    .ParetoOptimalMatchings(men, women)
                    //.PopularMatchings(men, women) Removing this because its not needed
                    .ToArray();
            });
        }
    }

    struct Matchings
    {
        public int n;
        public int min;
        public int max;
        public int[][] dominant;
        public int[][][] dominantPlusPlusEdges;
        public int[][] middle;
        public int[][] maxSizeNonDominant;
        public int[][] minSize;
        public int[][] paretoOptimalMatchings;
    }

    struct Scenario
    {
        public bool ideal;
        public PreferenceLists preferenceLists;
        public Matchings matchings;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("men:");
            sb.AppendLine(Utility.NewLineString(preferenceLists.men.Select(Utility.DefaultString)));
            sb.AppendLine();
            sb.AppendLine("women:");
            sb.AppendLine(Utility.NewLineString(preferenceLists.women.Select(Utility.DefaultString)));
            sb.AppendLine();

            sb.AppendFormat("# of unmatched men: [{0}, {1}]", matchings.min, matchings.max);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("Matchings:");

            sb.AppendLine("minSize:");
            sb.AppendLine(Utility.NewLineIndented(matchings.minSize.Select(Utility.DefaultString)));
            sb.AppendLine();

            var edgeFinder = Program.PlusPlusEdgeFinder(preferenceLists);
            sb.AppendLine("stable:");
            var stable = matchings.minSize.Where(m => edgeFinder(m).Count() == 0);
            sb.AppendLine(Utility.NewLineIndented(stable.Select(Utility.DefaultString)));
            sb.AppendLine();

            sb.AppendLine("maxSizeNonDominant:");
            sb.AppendLine(Utility.NewLineIndented(matchings.maxSizeNonDominant.Select(Utility.DefaultString)));
            sb.AppendLine();

            sb.AppendLine("middle:");
            sb.AppendLine(Utility.NewLineIndented(matchings.middle.Select(Utility.DefaultString)));
            sb.AppendLine();

            sb.AppendLine("dominant:");
            sb.AppendLine(Utility.NewLineIndented(matchings.dominant.Zip(matchings.dominantPlusPlusEdges, (matching, edges) =>
            {
                if(edges.Length > 0)
                {
                    Func<int[], string> edgeStringer = arr => Utility.CollectionToString(arr, "(", ", ", ")");
                    return Utility.DefaultString(matching) + ":" + Utility.CollectionToString(edges.Select(edgeStringer), "", ", ", "");
                }
                else
                {
                    return Utility.DefaultString(matching);
                }

            })));
            sb.AppendLine();

            sb.AppendLine("Gale Shapely:");
            sb.AppendLine(DiscreteKavitha.GaleShapley(preferenceLists.men, preferenceLists.women).DefaultString());
            sb.AppendLine();

            //sb.AppendLine(Program.makePolytopeExp(preferenceLists.men, matchings.dominant));

            sb.AppendLine("paretoOptimalMatchings:");
            sb.AppendLine(Utility.NewLineIndented(matchings.paretoOptimalMatchings.Select(Utility.DefaultString)));
            sb.AppendLine();

            List<List<int[][]>> intersectingPairs = new List<List<int[][]>>();

            List<int[]> firstMatchings = new List<int[]>();
            //firstMatchings.AddRange(matchings.dominant);
            //firstMatchings.AddRange(matchings.middle);
            //firstMatchings.AddRange(matchings.minSize);
            //firstMatchings.Add(DiscreteKavitha.GaleShapley(preferenceLists.men, preferenceLists.women));
            firstMatchings.AddRange(stable);

            sb.AppendLine("intersections(dominant):");
            foreach (var popMatch in firstMatchings)
            {
                foreach (var paretoMatch in matchings.paretoOptimalMatchings)
                {
                    int count = MatchingEqualityComparerByEdges.MatchingEdges(popMatch, paretoMatch);

                    while (intersectingPairs.Count <= count)
                        intersectingPairs.Add(new List<int[][]>());

                    intersectingPairs[count].Add(new int[2][] {
                        popMatch,
                        paretoMatch
                    });
                }
            }

            sb.AppendLine("intersections:");
            for (int i = 0; i < intersectingPairs.Count; i++)
            {
                sb.AppendLine("count = " + i);
                foreach(var pair in intersectingPairs[i])
                {
                    sb.AppendLine("\t" + Utility.DefaultString(pair[0]) + ", " + Utility.DefaultString(pair[1]));
                }
            }

            return sb.ToString();
        }
    }
}

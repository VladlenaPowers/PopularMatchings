using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
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

            //sb.AppendLine(Program.makePolytopeExp(preferenceLists.men, matchings.dominant));

            sb.AppendLine("paretoOptimalMatchings:");
            sb.AppendLine(Utility.NewLineIndented(matchings.paretoOptimalMatchings.Select(Utility.DefaultString)));
            sb.AppendLine();

            return sb.ToString();
        }
    }
}

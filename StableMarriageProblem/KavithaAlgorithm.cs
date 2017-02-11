using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableMarriageProblem
{
    class KavithaAlgorithm
    {
        // This function returns all integers in the sequence { 0, 1, 2, ... , n } that are not contained in the
        // arr parameter
        private static int[] ComplementIndicesArray(int[] arr, int n)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (!arr.Contains(i))
                {
                    output.Add(i);
                }
            }
            return output.ToArray();
        }

        public class Output
        {
            public int[] matching;
            public int[] men0;
            public int[] men1;

            public Output(int[] matching, int[] men1)
            {
                this.matching = matching;
                this.men0 = ComplementIndicesArray(men1, matching.Length);
                this.men1 = men1;
            }

            public class MatchingEqualityComparer : IEqualityComparer<Output>
            {
                public bool Equals(Output x, Output y)
                {
                    return Matching.equalityComparer.Equals(x.matching, y.matching);
                }

                public int GetHashCode(Output obj)
                {
                    return Matching.equalityComparer.GetHashCode(obj.matching);
                }
            }
            public static MatchingEqualityComparer matchingEqualityComparer = new MatchingEqualityComparer();
        }

        private struct IndexPair
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

        public static Output Run(int[][] men, int[][] women, IEnumerable<int> prioritizedMen)
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

            foreach (var man in prioritizedMen)
            {
                queuedMen.Enqueue(0, new IndexPair(1, man));
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

            while (!queuedMen.Empty())
            {
                IndexPair current = queuedMen.Dequeue();
                if (doubleMen[current.a][current.b].Count != 0)
                {
                    int mostPreferredNeighbor = doubleMen[current.a][current.b][0];
                    if (current.a == 1)
                    {
                        for (int j = 0; j < men.Length; j++)
                        {
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

                    List<int> womansPref = copyWomen[mostPreferredNeighbor];

                    int i = womansPref.IndexOf(current.b);
                    if (i == -1)
                    {
                        i = womansPref.Count - 1;
                    }
                    for (int j = womansPref.Count - 1; j > i; j--)
                    {
                        if (current.a == 0)
                        {
                            doubleMen[current.a][womansPref[j]].Remove(mostPreferredNeighbor);
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
                    queuedMen.Enqueue(0, new IndexPair(1, current.b));
                }
            }

            for (int i = 0; i < kavithaMatching.Length; i++)
            {
                if (kavithaMatching[i].b == -1)
                {
                    kavithaMatching[i].a = 1;
                }
            }

            int[] output = kavithaMatching.Select(pair => pair.b).ToArray();
            int[] men1 = kavithaMatching.Where(pair => pair.a == 1).Select((pair, i) => i).ToArray();
            return new Output(output, men1);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableMarriageProblem
{
    class Matching
    {
        //public class NonNegativeEntriesCComparer : IComparer<int[]>
        //{
        //    public int Compare(int[] x, int[] y)
        //    {
        //        int xE = NonNegativeEntries(x);
        //        int yE = NonNegativeEntries(y);

        //        return xE.CompareTo(yE);
        //    }
        //}
        //public static NonNegativeEntriesCComparer nonNegativeEntriesComparer = new NonNegativeEntriesCComparer();

        public class EqualityComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y)
            {
                if (x.Length != y.Length)
                {
                    throw new Exception("Matchings must be the same length");
                }
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            public int GetHashCode(int[] obj)
            {
                int output = 0;
                for (int i = 0; i < obj.Length; i++)
                {
                    output += i * obj[i];
                }
                return output;
            }
        }
        public static EqualityComparer equalityComparer = new EqualityComparer();

        public class PopularityComparer : IComparer<int[]>
        {
            int[][] men;
            int[][] women;

            public PopularityComparer(int[][] men, int[][] women)
            {
                this.men = men;
                this.women = women;
            }

            private static int ComparePairings(int[] pref, int x, int y)
            {
                if (x == y)
                {
                    return 0;
                }
                else if (x < 0)
                {
                    return -1;
                }
                else if (y < 0)
                {
                    return 1;
                }
                else
                {
                    int xRank = -1;
                    int yRank = -1;
                    for (int i = 0; i < pref.Length; i++)
                    {
                        int test = pref[i];
                        if (test == x)
                        {
                            xRank = i;
                            if (yRank >= 0)
                            {
                                break;
                            }
                        }
                        if (test == y)
                        {
                            yRank = i;
                            if (xRank >= 0)
                            {
                                break;
                            }
                        }
                    }

                    if (yRank < xRank)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            public int Compare(int[] x, int[] y)
            {
                //ask each person which matching they prefer

                int xScore = 0;
                int yScore = 0;

                for (int i = 0; i < men.Length; i++)
                {
                    int manPref = ComparePairings(men[i], x[i], y[i]);
                    if (manPref > 0)
                    {
                        xScore += 1;
                    }
                    else if (manPref < 0)
                    {
                        yScore += 1;
                    }
                }

                for (int i = 0; i < women.Length; i++)
                {
                    int indexX = -1;
                    int indexY = -1;
                    for (int j = 0; j < x.Length; j++)
                    {
                        if (x[j] == i)
                        {
                            indexX = j;
                            if (indexY >= 0)
                            {
                                break;
                            }
                        }
                        if (y[j] == i)
                        {
                            indexY = j;
                            if (indexX >= 0)
                            {
                                break;
                            }
                        }
                    }
                    int womanPref = ComparePairings(women[i], indexX, indexY);
                    if (womanPref > 0)
                    {
                        xScore += 1;
                    }
                    else if (womanPref < 0)
                    {
                        yScore += 1;
                    }
                }

                if (xScore < yScore)
                {
                    return -1;
                }
                else if (yScore < xScore)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}

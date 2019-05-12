using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PopularMatching
{
    //Compares two matchings to determine which one is more popular given the preference lists of the men and women
    public class MatchingPopularityComparer : IComparer<int[]>
    {
        int[][] men;
        int[][] women;

        int[][] womenRank;
        int[][] menRank;

        public MatchingPopularityComparer(int[][] men, int[][] women)
        {
            this.men = men;
            this.women = women;

            int menLength = men.Length;
            int womenLength = women.Length;

            menRank = new int[menLength][];
            womenRank = new int[womenLength][];

            for (int i = 0; i < menLength; i++)
            {
                menRank[i] = new int[womenLength];
                for (int j = 0; j < womenLength; j++)
                {
                    int index = Array.IndexOf(men[i], j);
                    menRank[i][j] = (index < 0) ? int.MaxValue : index;
                }

                womenRank[i] = new int[menLength];
                for (int j = 0; j < menLength; j++)
                {
                    int index = Array.IndexOf(women[i], j);
                    womenRank[i][j] = (index < 0) ? int.MaxValue : index;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ComparePairings(int[] ranks, int x, int y)
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
                int xRank = ranks[x];
                int yRank = ranks[y];

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
                int manPref = ComparePairings(menRank[i], x[i], y[i]);
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
                //int indexX = Array.IndexOf(x, i);
                //int indexY = Array.IndexOf(y, i);
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
                int womanPref = ComparePairings(womenRank[i], indexX, indexY);
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

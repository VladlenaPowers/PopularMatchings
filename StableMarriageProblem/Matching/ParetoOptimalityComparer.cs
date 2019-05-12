using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
    public class ParetoOptimalityComparer : IComparer<int[]>
    {
        int[][] men;
        int[][] women;

        int[][] menRank;

        public ParetoOptimalityComparer(int[][] men, int[][] women)
        {
            this.men = men;
            this.women = women;

            int menLength = men.Length;
            int womenLength = women.Length;

            menRank = new int[menLength][];

            for (int i = 0; i < menLength; i++)
            {
                menRank[i] = new int[womenLength];
                for (int j = 0; j < womenLength; j++)
                {
                    int index = Array.IndexOf(men[i], j);
                    menRank[i][j] = (index < 0) ? int.MaxValue : index;
                }
            }
        }

        public int Compare(int[] x, int[] y)
        {
            //ask each person which matching they prefer
            
            for (int i = 0; i < men.Length; i++)
            {
                if (x[i] == y[i])
                {
                    continue;
                }
                else if (y[i] == -1)
                {
                    return 1;
                }
                else if (x[i] == -1)
                {
                    continue;
                }
                else if(menRank[i][x[i]] > menRank[i][y[i]])
                {
                    continue;
                }
                else
                {
                    return 1;
                }
            }

            return -1;
        }
    }
}

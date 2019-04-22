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

        public ParetoOptimalityComparer(int[][] men, int[][] women)
        {
            this.men = men;
            this.women = women;
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
                else if(Array.IndexOf(men[i], x[i]) > Array.IndexOf(men[i], y[i]))
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

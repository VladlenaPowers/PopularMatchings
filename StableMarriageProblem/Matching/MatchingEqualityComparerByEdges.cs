﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
    //compares two matchings to see if they are the same.
    public class MatchingEqualityComparerByEdges : IEqualityComparer<int[]>
    {
        public static int MatchingEdges(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                throw new Exception("Matchings must be the same length");
            }

            int count = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == y[i] && x[i] >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        public bool Equals(int[] x, int[] y)
        {
            return MatchingEqualityComparerByEdges.MatchingEdges(x, y) <= 1;
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

        public static MatchingEqualityComparerByEdges INSTANCE = new MatchingEqualityComparerByEdges();
    }
}

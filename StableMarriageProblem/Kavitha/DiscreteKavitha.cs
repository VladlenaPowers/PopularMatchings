using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopularMatching
{
    class DiscreteKavitha
    {

        public class Output
        {
            public int[] matching;
            public int[] men0;
            public int[] men1;

            public Output(int[] matching, int[] men1)
            {
                this.matching = matching;
                this.men0 = Enumerable.Range(0, matching.Length).Where(manI => !men1.Contains(manI)).ToArray();
                this.men1 = men1;
            }
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

        private struct Pairing
        {
            public int manI, womanI;
            public Pairing(int manI, int womanI)
            {
                this.manI = manI;
                this.womanI = womanI;
            }
            public override string ToString()
            {
                return manI + ", " + womanI;
            }
        }

        public static int[] GaleShapley(int[][] men, int[][] women)
        {
            IEnumerable<int>[] menPrefs = men.Select(l => l.Skip(0)).ToArray();
            IEnumerable<int>[] womenPrefs = women.Select(l => l.Skip(0)).ToArray();

            //Pairing[] matching = Enumerable.Range(0, men.Length).Select(i => new Pairing(i, -1)).ToArray();
            List<int> matching = new List<int>(men.Length);

            var queue = new Queue<int>();
            foreach (var manI in Enumerable.Range(0, men.Length))
            {
                matching.Add(-1);
                queue.Enqueue(manI);
            }

            //while (matching.Any(pairing => (pairing.womanI < 0) && (menPrefs[pairing.manI].Count() > 0)))
            while (queue.Count > 0)
            {
                int manI = queue.Dequeue();

                IEnumerable<int> prefList = menPrefs[manI];
                if (prefList.Count() > 0)
                {
                    int womanI = prefList.ElementAt(0);
                    int alreadyMatchedManI = matching.IndexOf(womanI);
                    if (alreadyMatchedManI >= 0)
                    {
                        queue.Enqueue(alreadyMatchedManI);
                        matching[alreadyMatchedManI] = -1;
                    }
                    matching[manI] = womanI;//match man with most prefered woman
                    menPrefs[manI] = prefList.Skip(1);//remove woman from preference list

                    List<int> womanPrefList = womenPrefs[womanI].ToList();
                    int indexOfMan = womanPrefList.IndexOf(manI);
                    foreach (var otherManI in womanPrefList.Skip(indexOfMan+1))
                    {
                        menPrefs[otherManI] = menPrefs[otherManI].Where(wI => wI != womanI);
                    }
                    womenPrefs[womanI] = womanPrefList.Take(indexOfMan);
                }
            }

            //return matching.Select(pairing => pairing.womanI).ToArray();
            return matching.ToArray();
        }

        public static Output Run(int[][] men, int[][] women, IEnumerable<int> prioritizedMen)
        {
            //remove unprioritized men from womens preference lists
            var womenPrefWithoutMen1 = women.Select(pl => pl.Where(manI => prioritizedMen.Contains(manI)).ToArray()).ToArray();

            //create preference lists for prioritized and unprioritized men
            var prioritizedMenPref = men.Select((pl, i) => prioritizedMen.Contains(i) ? pl : new int[0]).ToArray();
            var nonPrioritizedMenPref = men.Select((pl, i) => !prioritizedMen.Contains(i) ? pl : new int[0]).ToArray();

            //Run for prioritized men
            int[] prioritizedMatching = GaleShapley(prioritizedMenPref, womenPrefWithoutMen1);

            //remove all women who are matched with prioritized men from the non-prioritized mens preference lists
            nonPrioritizedMenPref = nonPrioritizedMenPref.Select(pl => pl.Where(womanI => !prioritizedMatching.Contains(womanI)).ToArray()).ToArray();

            //remove prioritized men from womens preference lists
            var womenPrefWithoutMen0 = women.Select(pl => pl.Where(manI => !prioritizedMen.Contains(manI)).ToArray()).ToArray();

            //Run for unprioritized men
            int[] unprioritizedMatching = GaleShapley(nonPrioritizedMenPref, womenPrefWithoutMen0);
            Pairing[] pMathcing = unprioritizedMatching.Zip(Enumerable.Range(0, men.Length), (womanI, manI) => new Pairing(manI, womanI)).ToArray();
            
            if (pMathcing.Any(pairing => !prioritizedMen.Contains(pairing.manI) && (pairing.womanI < 0)))
            {
                //atleast one man from the unprioritized man list is unmatched

                var unmatchedMen = pMathcing.Where(pairing => !prioritizedMen.Contains(pairing.manI) && (pairing.womanI < 0)).Select(pairing => pairing.manI).ToArray();
                var nPrioritizedMen = prioritizedMen.Concat(unmatchedMen).ToArray();

                return DiscreteKavitha.Run(men, women, nPrioritizedMen);
            }
            else
            {
                var unprioritizedMen = Enumerable.Range(0, men.Length).Where(manI => !prioritizedMen.Contains(manI));


                var output = new int[men.Length];
                foreach (var manI in prioritizedMen) {
                    output[manI] = prioritizedMatching[manI];
                }
                foreach (var manI in unprioritizedMen) {
                    output[manI] = unprioritizedMatching[manI];
                }
                
                return new Output(output, prioritizedMen.ToArray());
            }
        }
    }
}

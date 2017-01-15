using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableMarriageProblem
{
    class PriorityQueue<T>
    {
        private List<Queue<T>> queues;

        public PriorityQueue()
        {
            queues = new List<Queue<T>>();
        }

        public void Enqueue(int priority, T val)
        {
            while (queues.Count <= priority)
            {
                queues.Add(new Queue<T>());
            }

            queues[priority].Enqueue(val);
        }

        public T Dequeue()
        {
            for (int i = 0; i < queues.Count; i++)
            {
                if (queues[i].Count > 0)
                {
                    return queues[i].Dequeue();
                }
            }

            throw new InvalidOperationException();
        }

        public bool Empty()
        {
            for (int i = 0; i < queues.Count; i++)
            {
                if (queues[i].Count > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

using System.Collections.Generic;

namespace Runtime.DataStructure
{
    public class PriorityQueue<T, Y> where Y : struct
    {
        private List<(T element, Y priority)> elements = new List<(T, Y)>();
        public int Count => elements.Count;
        public void Clear() => elements.Clear();

        public void Enqueue(T _item, Y _priority)
        {
            elements.Insert(0, (_item, _priority));
        }

        static readonly Comparer<Y> comparer = Comparer<Y>.Default;

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (comparer.Compare(elements[i].priority, elements[bestIndex].priority) < 0)
                    bestIndex = i;
            }

            var bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
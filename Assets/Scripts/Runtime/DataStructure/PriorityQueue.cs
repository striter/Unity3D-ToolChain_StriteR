using System;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.DataStructure
{
    public class PriorityQueue<Element, Comparerer> where Comparerer : IComparable<Comparerer>
    {
        static readonly Comparer<Comparerer> comparer = Comparer<Comparerer>.Default;
        private Dictionary<Element,Comparerer> elements = new Dictionary<Element, Comparerer>();
        public int Count => elements.Count;
        public void Clear() => elements.Clear();

        public void Enqueue(Element _item, Comparerer _priority)
        {
            elements.TryAdd(_item, _priority);
            elements[_item] = _priority;
        }

        public Element Dequeue()
        {
            var least = elements.First();
            var minimumKey = least.Key;
            var leastValue = least.Value;
            foreach (var pair in elements)
            {
                if (comparer.Compare(pair.Value, leastValue) < 0)
                    minimumKey = pair.Key;
            }
            elements.Remove(minimumKey);
            return minimumKey;
        }
    }
}
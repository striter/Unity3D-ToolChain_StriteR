using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.DataStructure
{
    public class PriorityQueue<Element, Comparable> : IEnumerable<KeyValuePair<Element, Comparable>> where Comparable : IComparable<Comparable> 
    {
        static readonly Comparer<Comparable> comparer = Comparer<Comparable>.Default;
        private Dictionary<Element,Comparable> elements = new Dictionary<Element, Comparable>();
        public int Count => elements.Count;
        public void Clear() => elements.Clear();

        public void Enqueue(Element _item, Comparable _priority)
        {
            elements.TryAdd(_item, _priority);
            elements[_item] = _priority;
        }

        public Element Peek()
        {
            var least = elements.First();
            var minimumKey = least.Key;
            var leastValue = least.Value;
            foreach (var pair in elements)
            {
                if (comparer.Compare(pair.Value, leastValue) < 0)
                    minimumKey = pair.Key;
            }
            return minimumKey;
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

        public IEnumerator<KeyValuePair<Element, Comparable>> GetEnumerator() => elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
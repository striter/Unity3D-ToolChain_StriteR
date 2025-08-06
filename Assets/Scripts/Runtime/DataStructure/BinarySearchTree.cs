using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.DataStructure
{
    [Serializable]
    public class BinarySearchTree<Element> where Element : IComparable<Element>
    {
        [Serializable]
        public struct Node
        {
            public Element element;
            public int leftIndex, rightIndex;
        }
    
        [field : SerializeField] public List<Node> nodes { get; private set; } = new List<Node>();
        public int Count => nodes.Count;
        
        public void Clear() => nodes.Clear();
        
        public void Add(Element _element)
        {
            nodes.Add(new Node() {
                element = _element,
                leftIndex = -1,
                rightIndex = -1
            });
            if (nodes.Count == 1)
                return;
            
            var childIndex = nodes.Count - 1;
            var parentIndex = 0;
            while (parentIndex != -1)
            {
                var parent = nodes[parentIndex];
                var compare = _element.CompareTo(parent.element);
                var nextIndex = -1;
                if (compare < 0)
                {
                    nextIndex = parent.leftIndex;
                    if (nextIndex == -1)
                        parent.leftIndex = childIndex;
                }
                else
                {
                    nextIndex = parent.rightIndex;
                    if (nextIndex == -1)
                        parent.rightIndex = childIndex;
                }
                nodes[parentIndex] = parent;
                parentIndex = nextIndex;
            }
        }
    }

}
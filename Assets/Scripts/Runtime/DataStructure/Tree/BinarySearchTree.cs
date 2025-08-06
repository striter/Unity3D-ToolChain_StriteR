using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
            public static readonly Node kInvalid = new(){leftIndex = -1, rightIndex = -1};
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
        
        
        //https://www.geeksforgeeks.org/dsa/how-to-determine-if-a-binary-tree-is-balanced/
        int isBalanced(Node root)
        {
            if (root is { leftIndex: -1, rightIndex: -1 })
                return 0;
            
            var lHeight = isBalanced(root.leftIndex == -1 ? Node.kInvalid : nodes[root.leftIndex]);
            var rHeight = isBalanced(root.rightIndex == -1 ? Node.kInvalid : nodes[root.rightIndex]);

            if (lHeight == -1 || rHeight == -1 || math.abs(lHeight - rHeight) > 1)
                return -1;

            return math.max(lHeight, rHeight) + 1;
        }

        public bool IsBalanced()
        {
            if (nodes.Count == 0)
                return true;
            
            var root = nodes[0];
            return isBalanced(root) > 0;
        }
    }

}
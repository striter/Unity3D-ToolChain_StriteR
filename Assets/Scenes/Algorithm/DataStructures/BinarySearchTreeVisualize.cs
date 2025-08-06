using System;
using System.Collections.Generic;
using Runtime.DataStructure;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.DataStructures
{
    public class BinarySearchTreeVisualize : MonoBehaviour
    {
        public BinarySearchTree<int> m_Tree = new();
        
        [InspectorButton(true)]
        public void Insert(int _element) => m_Tree.Add(_element);
        
        [InspectorButton(true)]
        public void Clear() => m_Tree.Clear();

        private void OnDrawGizmos()
        {
            var nodes = m_Tree.nodes;
            if (nodes.Count == 0)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetA(.1f);
            var stack = new Stack<(int,float2,BinarySearchTree<int>.Node)>();
            stack.Push((0,float2.zero,nodes[0]));
            while (stack.Count > 0)
            {
                var (depth,position,node) = stack.Pop();
                if (node.leftIndex != -1)
                {
                    var newPosition = position + kfloat2.down + kfloat2.left / (depth+1);
                    Gizmos.DrawLine(position.to3xz(),newPosition.to3xz());
                    stack.Push((depth + 1,newPosition ,nodes[node.leftIndex]));
                }

                if (node.rightIndex != -1)
                {
                    var newPosition = position + kfloat2.down + kfloat2.right / (depth+1);
                    Gizmos.DrawLine(position.to3xz(),newPosition.to3xz());
                    stack.Push((depth + 1,newPosition,nodes[node.rightIndex]));
                }
                Gizmos.DrawWireSphere(position.to3xz(),.1f);
                UGizmos.DrawString(node.element.ToString(), position.to3xz());
            }
        }
    }
}
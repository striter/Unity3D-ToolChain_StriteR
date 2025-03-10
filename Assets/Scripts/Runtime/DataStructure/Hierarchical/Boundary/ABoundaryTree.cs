using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Scripting;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public abstract class ABoundaryTree<Boundary,Element> where Boundary : struct
    {
        public struct Node
        {
            public Boundary boundary;
            public int iteration;
            public List<int> elementsIndex;
            public List<int> childNodeIndex;
            public bool IsParent => childNodeIndex.Count > 0;
            private static ListPool<int> kIndexPool = new();

            public static Node Spawn() => new() {
                iteration = -1,
                boundary = default,
                childNodeIndex = kIndexPool.Spawn(),
                elementsIndex = kIndexPool.Spawn(),
            };

            public static Node Spawn(int _iteration)
            {
                var node = Spawn();
                node.iteration = _iteration;
                return node;
            }

            public static Node Spawn(int _iteration, Boundary _boundary)
            {
                var node = Spawn();
                node.iteration = _iteration;
                node.boundary = _boundary;
                return node;
            }

            public static void Recycle(Node _node)
            {
                kIndexPool.Despawn(_node.elementsIndex);
                _node.elementsIndex = null;
                kIndexPool.Despawn(_node.childNodeIndex);
                _node.childNodeIndex = null;
            }

            public T Index<T>(IList<T> _elements, int _index) => _elements[elementsIndex[_index]];
            public IEnumerable<T> ForEach<T>(IList<T> _elements)
            {
                for (var i = 0; i < elementsIndex.Count; i++)
                    yield return _elements[elementsIndex[i]];
            }
        }

        public int m_NodeCapacity = 16;
        public int m_MaxIteration = 4;
        private List<Node> m_Nodes = new List<Node>();

        public ABoundaryTree(int _nodeCapcity, int _maxIteration)
        {
            m_NodeCapacity = _nodeCapcity;
            m_MaxIteration = _maxIteration;
        }
        void Clear()
        {
            m_Nodes.ForEach(Node.Recycle);
            m_Nodes.Clear();
        }

        public void Dispose()
        {
            Clear();
            m_Nodes = null;
        }
        protected abstract IEnumerable<Node> Split(Node _parent, IList<Element> _elements);
        public void Construct(Boundary _boundary,IList<Element> _elements)
        {
            Clear();
            var rootChildCount = _elements.Count;
            var rootNode = Node.Spawn(0,_boundary);
            for(var i=0;i<rootChildCount;i++)
                rootNode.elementsIndex.Add(i);
            m_Nodes.Add(rootNode);
            
            var finalChildCount = rootChildCount;
            var constructing = true;
            while (constructing)
            {
                var split = false;
                foreach (var node in from node in m_Nodes where !node.IsParent && node.iteration < m_MaxIteration && node.elementsIndex.Count > m_NodeCapacity select node)
                {
                    finalChildCount -= node.elementsIndex.Count;
                    foreach (var childNode in Split(node,_elements))
                    {
                        finalChildCount += childNode.elementsIndex.Count;
                        node.childNodeIndex.Add(m_Nodes.Count);
                        m_Nodes.Add(childNode);
                    }
                    
                    node.elementsIndex.Clear();
                    split = true;
                    break;
                }

                constructing = split;
                if (m_Nodes[^1].iteration > 1024 || m_Nodes.Count > 2048)
                {
                    Debug.LogError("Tree Construct Exception: Maximum Iteration Reached");
                    break;
                }
            }
            
            if(finalChildCount != rootChildCount)
                Debug.LogError($"Tree element loss detected, finalChildCount != childCount {finalChildCount} != {rootChildCount}");
        }
        
        public IEnumerable<Node> GetLeafs() => from node in m_Nodes where !node.IsParent select node;
        public IEnumerable<Node> GetParents() => from node in m_Nodes where node.IsParent select node;

        private Stack<Node> m_QueryNodes = new();
        public IEnumerable<Element> Query<Element>(IList<Element> _elements,Predicate<Boundary> _queryFunction)
        {
            if (m_Nodes.Count == 0)
                yield break;
            
            m_QueryNodes.Clear();
            m_QueryNodes.Push(m_Nodes[0]);
            while (m_QueryNodes.Count > 0)
            {
                var currentTreeNode = m_QueryNodes.Pop();
                if(!_queryFunction(currentTreeNode.boundary))
                    continue;

                if (currentTreeNode.IsParent)
                {
                    for(var i = currentTreeNode.childNodeIndex.Count - 1 ;i >= 0; i--)
                        m_QueryNodes.Push(m_Nodes[currentTreeNode.childNodeIndex[i]]);
                }

                if (currentTreeNode.elementsIndex == null) continue;
                
                foreach (var element in currentTreeNode.elementsIndex)
                    yield return _elements[element];
            }
        }
        
        public IEnumerator<Element> ForEach<Element>(IList<Element> _elements)
        {
            foreach (var volume in m_Nodes)
            {
                if(volume.IsParent)
                    continue;
                foreach (var elementIndex in volume.elementsIndex)
                    yield return _elements[elementIndex];
            }
        }
        
        #if UNITY_EDITOR
        public void DrawHandles(IList<Element> _elements,bool _parentMode = false)
        {
            if (_parentMode)
            {
                foreach (var node in GetParents())
                {
                    UnityEditor.Handles.color = UColor.IndexToColor(node.iteration).SetA(.2f);
                    if (node.boundary is IHandles boundsShape)
                        boundsShape.DrawHandles();
                }
            }
            else
            {
                var index = 0;
                foreach (var leaf in GetLeafs())
                {
                    UnityEditor.Handles.color = UColor.IndexToColor(index++);
                    if (leaf.boundary is IHandles boundsShape)
                        boundsShape.DrawHandles();
                    foreach (var element in leaf.elementsIndex)
                        switch (_elements[element])
                        {
                            case IHandles handles:
                                handles.DrawHandles();
                                break;
                            case float3 v:
                                UnityEditor.UHandles.DrawWireSphere(v,0.1f);
                                break;
                            case float2 v2:
                                UnityEditor.UHandles.DrawWireSphere(v2.to3xz(),0.1f);
                                break;
                        }
                }
            }
            
        }
        #endif
        
        public virtual void DrawGizmos(IList<Element> _elements, bool _parentMode = false)
        {
            if (_parentMode)
            {
                foreach (var node in GetParents())
                {
                    Gizmos.color = UColor.IndexToColor(node.iteration).SetA(.2f);
                    if (node.boundary is IGizmos boundsShape)
                        boundsShape.DrawGizmos();
                }
            }
            else
            {
                var index = 0;
                foreach (var leaf in GetLeafs())
                {
                    Gizmos.color = UColor.IndexToColor(index++);
                    switch (leaf.boundary)
                    {
                        case IGizmos gizmos:
                            gizmos.DrawGizmos();
                            break;
                    }
                    
                    foreach (var element in leaf.elementsIndex)
                        switch (_elements[element])
                        {
                            case IGizmos gizmos:
                                gizmos.DrawGizmos();
                                break;
                            case float3 v:
                                Gizmos.DrawSphere(v,0.1f);
                                break;
                            case float2 v2:
                                Gizmos.DrawSphere(v2.to3xz(),0.1f);
                                break;
                        }
                }
            }
        }
    }
}
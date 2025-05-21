using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public interface IBoundaryTreeQuery<Boundary,Element>
    {
        bool Query(Boundary _boundary);
        bool Query(int _index,Element _element);
    }

    public static class IBoundaryTreeQuery_Extension
    {
        public static List<Element> Query<Boundary, Element>(this IBoundaryTreeQuery<Boundary, Element> _query, ABoundaryTree<Boundary,Element> _boundaryTree,IList<Element> _elements) where Boundary:struct =>_boundaryTree.Query(_elements, _query);
    }
    
    public abstract class ABoundaryTree<Boundary,Element> where Boundary : struct
    {
        public struct Node
        {
            public Boundary boundary;
            public int iteration;
            public List<int> elementsIndex;
            public List<int> childNodeIndex;
            public bool IsParent => childNodeIndex.Count > 0;
            public int ElementCount => elementsIndex.Count;
            private static PoolList<int> kIndexPool = new();

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

            public T Index<T>(IList<T> _elements, int _treeIndex, out int _elementIndex)
            {
                _elementIndex = elementsIndex[_treeIndex];
                return _elements[_elementIndex];
            }
            public List<T> FillList<T>(IList<T> _elements, List<T> _list)
            {
                _list.Clear();
                for (var i = 0; i < elementsIndex.Count; i++)
                    _list.Add(_elements[elementsIndex[i]]);
                return _list;
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

        protected abstract void Split(Node _parent, IList<Element> _elements, List<Node> _nodeList);
        private static List<Node> kNodeHelper = new();
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
                for(var i= m_Nodes.Count -1;i>=0;i--)
                {
                    var node = m_Nodes[i];
                    if (node.IsParent || 
                        node.iteration >= m_MaxIteration ||
                        node.elementsIndex.Count <= m_NodeCapacity)
                        continue;
                    
                    finalChildCount -= node.elementsIndex.Count;
                    kNodeHelper.Clear();
                    Split(node, _elements, kNodeHelper);
                    foreach (var childNode in kNodeHelper)
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

        private struct FDefaultQuery : IBoundaryTreeQuery<Boundary,Element>
        {
            public Predicate<Boundary> queryBoundary;
            public Func<int,Element,bool> queryElement;

            public bool Query(Boundary _boundary) => queryBoundary(_boundary);
            public bool Query(int _index,Element _element) => queryElement!=null ? queryElement(_index,_element) : true;
        }

        private List<Element> kQueryTempElements = new();
        public List<Element> Query(IList<Element> _elements ,Predicate<Boundary> _queryBoundary, Func<int,Element,bool> _queryElement = null) => Query(_elements, new FDefaultQuery { queryBoundary = _queryBoundary, queryElement = _queryElement },kQueryTempElements);
        public List<Element> Query(IList<Element> _elements, IBoundaryTreeQuery<Boundary, Element> _query) => Query(_elements, _query, kQueryTempElements);
        
        private Stack<Node> kQueryNodes = new();
        public List<Element> Query(IList<Element> _elements,IBoundaryTreeQuery<Boundary, Element> _query,List<Element> _queryList)
        {
            _queryList.Clear();
            if (m_Nodes.Count == 0)
                return _queryList;
            
            kQueryNodes.Clear();
            kQueryNodes.Push(m_Nodes[0]);
            while (kQueryNodes.Count > 0)
            {
                var currentTreeNode = kQueryNodes.Pop();
                if(!_query.Query(currentTreeNode.boundary))
                    continue;

                if (currentTreeNode.IsParent)
                {
                    for(var i = currentTreeNode.childNodeIndex.Count - 1 ;i >= 0; i--)
                        kQueryNodes.Push(m_Nodes[currentTreeNode.childNodeIndex[i]]);
                    continue;
                }

                for (var i = currentTreeNode.ElementCount - 1; i >= 0; i--)
                {
                    var element = currentTreeNode.Index(_elements, i,out var index);
                    if (_query.Query(index,element))
                        _queryList.Add(element);
                }
            }
            return _queryList;
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
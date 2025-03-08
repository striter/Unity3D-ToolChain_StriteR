using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public abstract class ABoundaryTree<Boundary,Element> : BinaryTree<ABoundaryTree<Boundary,Element>.BoundaryTreeNode,Element> where Boundary : struct
    {
        public class BoundaryTreeNode : ITreeNode<BoundaryTreeNode,Element>
        {
            public int iteration { get; set; }
            public Boundary boundary { get; set; }
            public IList<Element> elements { get; set; }
            public IList<BoundaryTreeNode> children { get; set; }
        }

        protected abstract IEnumerable<(Boundary,IList<Element>)> Split(int _iteration,Boundary _boundary, IList<Element> _elements);
        public void Construct(IList<Element> _elements,Boundary _boundary, int _maxIteration, int _volumeCapacity) => Construct_Internal(new BoundaryTreeNode()
        {
            iteration = 0,
            elements = _elements,
            boundary = _boundary,
        }, _maxIteration, _volumeCapacity, Split);
        IEnumerable<BoundaryTreeNode> Split(BoundaryTreeNode _node)
        {
            var iteration = _node.iteration + 1;
            foreach (var (boundary,elements) in Split(iteration,_node.boundary,_node.elements))
            {
                yield return new BoundaryTreeNode()
                {
                    boundary = boundary,
                    elements = elements,
                    iteration = iteration,
                    children = null,
                };
            }
        }
        public virtual void DrawGizmos(bool _parentMode = false)
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
                    
                    foreach (var element in leaf.elements)
                        switch (element)
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
        
        #if UNITY_EDITOR
        public void DrawHandles(bool _parentMode = false)
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
                    foreach (var element in leaf.elements)
                        switch (element)
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
    }
}
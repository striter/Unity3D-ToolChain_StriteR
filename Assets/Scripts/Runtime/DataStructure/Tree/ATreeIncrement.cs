using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public interface ITreeNode<Boundary, Element> where Boundary : struct where Element : struct
    {
        int iteration { get; set; }
        Boundary boundary { get; set; }
        IList<Element> elements { get; set; }
        public Boundary CalculateBounds(IEnumerable<Element> _elements);
        public bool Contains(Boundary _bounds, Element _element);
    }

    public abstract class ATreeIncrement<Node, Boundary, Element> : IEnumerable<Node>
        where Node : struct, ITreeNode<Boundary, Element>
        where Boundary : struct
        where Element : struct
    {
        protected List<Node> m_Nodes { get; private set; } = new List<Node>();

        private Node kHelper = default;
        public void Construct(IEnumerable<Element> _elements, int _nodeCapacity, int _maxIteration,bool _clearEmpty = true,bool _recalculateBounds = true)
        {
            var elements = _elements.ToList();
            m_Nodes.Clear();
            m_Nodes.Add(new Node()
            {
                iteration = 0,
                boundary = kHelper.CalculateBounds(elements),
                elements = elements,
            });

            PostOnConstruct(_nodeCapacity,_maxIteration,_clearEmpty,_recalculateBounds);
        }

        public void ConstructGeometry(Boundary _initailBoundary,int _maxIteration)
        {
            m_Nodes.Clear();
            m_Nodes.Add(new Node()
            {
                iteration = 0,
                boundary = _initailBoundary,
                elements = new List<Element>(){default},
            });

            PostOnConstruct(0,_maxIteration,false,false);
        }
        

        void PostOnConstruct( int _nodeCapacity, int _maxIteration,bool _clearEmpty,bool recalculateBounds)
        {
            var constructionFinished = false;
            while(!constructionFinished)
            {
                var constructing = false;
                for (int i = 0; i < m_Nodes.Count; i++)
                {
                    var node = m_Nodes[i];
                    if (node.iteration >= _maxIteration
                        || node.elements.Count <= _nodeCapacity
                       )
                        continue;

                    var newIteration = node.iteration + 1;
                    foreach (var boundary in Divide(node.boundary, node.iteration))
                    {
                        var newElements = node.elements.Collect(p => kHelper.Contains(boundary, p)).ToList();

                        m_Nodes.Add(new Node()
                        {
                            iteration = newIteration,
                            boundary = recalculateBounds ? kHelper.CalculateBounds(newElements) : boundary,
                            elements = newElements
                        });
                    }

                    m_Nodes.RemoveAt(i);
                    i--;
                    constructing = true;
                }

                constructionFinished = !constructing;
            }

            if (_clearEmpty)
            {
                for (int i = m_Nodes.Count - 1; i >= 0; i--)
                    if (m_Nodes[i].elements.Count == 0)
                        m_Nodes.RemoveAt(i);
            }
        }
        
        
        protected abstract IEnumerable<Boundary> Divide(Boundary _src, int _iteration);

        public void DrawGizmos(bool _boundary = true,bool _elements = true)
        {
            int index = 0;
            var matrix = Gizmos.matrix;
            foreach (var node in m_Nodes)
            {
                Gizmos.color = UColor.IndexToColor(index++);
                Gizmos.matrix = matrix;
                if (_boundary)
                {
                    if (node.boundary is IShapeGizmos boundsShape)
                        boundsShape.DrawGizmos();
                }

                if (_elements)
                {
                    foreach (var element in node.elements)
                        if (element is IShapeGizmos gizmos)
                            gizmos.DrawGizmos();
                        else if (element is float3 _val3)
                            Gizmos.DrawSphere(_val3, .1f);
                        else if (element is float2 _val2)
                            Gizmos.DrawSphere(_val2.to3xz(), .1f);
                }
            }

            Gizmos.matrix = matrix;
        }

        public IEnumerator<Node> GetEnumerator() => m_Nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

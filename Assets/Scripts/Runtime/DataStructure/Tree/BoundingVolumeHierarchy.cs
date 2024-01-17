using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Runtime.DataStructure
{
    public interface IBVHNode<Bounds, Element> : ITreeNode<Bounds, Element>
        where Bounds : struct
        where Element : struct
    {
        public void SortElements(int _median, IList<Element> _elements);
    }

    public class BoundingVolumeHierarchy<Node, Boundary, Element> 
        where Node : struct, IBVHNode<Boundary, Element>
        where Boundary : struct
        where Element : struct
    {
        private Node kHelper = default;
        public List<Node> m_Volumes { get; private set; } = new List<Node>();

        void Divide(Node _volume, out Node _node1, out Node _node2)
        {
            var last = _volume.elements.Count;
            var median = _volume.elements.Count / 2;
            _volume.SortElements(median, _volume.elements);

            var nextIteration = _volume.iteration + 1;
            _node1 = default;
            _node1.iteration = nextIteration;
            _node1.elements = new List<Element>(_volume.elements.Iterate(0, median));
            _node1.boundary = kHelper.CalculateBounds(_node1.elements);

            _node2 = default;
            _node2.iteration = nextIteration;
            _node2.elements = new List<Element>(_volume.elements.Iterate(median, last));
            _node2.boundary = kHelper.CalculateBounds(_node2.elements);
        }

        public void Construct(IList<Element> _elements, int _maxIteration, int _volumeCapacity)
        {
            m_Volumes.Clear();

            Node initial = default;
            initial.iteration = 0;
            initial.boundary = kHelper.CalculateBounds(_elements);
            initial.elements = _elements;

            m_Volumes.Add(initial);

            bool doBreak = true;
            while (doBreak)
            {
                bool split = false;
                for (int i = 0; i < m_Volumes.Count; i++)
                {
                    var node = m_Volumes[i];
                    if (node.iteration >= _maxIteration)
                        continue;

                    if (node.elements.Count <= _volumeCapacity)
                        continue;

                    Divide(node, out var node1, out var node2);
                    m_Volumes.Add(node1);
                    m_Volumes.Add(node2);

                    m_Volumes.RemoveAt(i);
                    split = true;
                    break;
                }

                doBreak = split;
            }
        }

        public void DrawGizmos()
        {
            int index = 0;
            var matrix = Gizmos.matrix;
            foreach (var node in m_Volumes)
            {
                Gizmos.color = UColor.IndexToColor(index++);
                Gizmos.matrix = matrix;
                if (node.boundary is IShapeGizmos boundsShape)
                    boundsShape.DrawGizmos();
                foreach (var element in node.elements)
                    if (element is IShapeGizmos gizmos)
                        gizmos.DrawGizmos();
            }

            Gizmos.matrix = matrix;
        }
    }
}


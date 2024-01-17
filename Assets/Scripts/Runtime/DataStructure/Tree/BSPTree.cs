using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.DataStructure
{
    public class BSPTreeNode
    {
        public int m_Iteration;
        public G2Plane m_Plane;
        public List<float2> m_Points;
        public bool front;
    }

    public class BSPTree
    {
        private int m_MaxIteration;
        private int m_NodeCapacity;

        public List<BSPTreeNode> m_Nodes = new List<BSPTreeNode>();

        public void Divide(IList<float2> _points, out G2Plane _plane, out List<float2> _front, out List<float2> _back)
        {
            PrincipleComponentAnalysis.Evaluate(_points, out var centre, out var right, out var up);
            _plane = new G2Plane(up, centre);
            _front = new List<float2>();
            _back = new List<float2>();
            foreach (var point in _points)
            {
                if (_plane.IsPointFront(point))
                    _front.Add(point);
                else
                    _back.Add(point);
            }
        }

        public void Construct(IList<float2> _points, int _maxIteration, int _nodeCapacity)
        {
            m_Nodes.Clear();
            m_Nodes.Add(new BSPTreeNode()
            {
                m_Iteration = 0,
                m_Plane = G2Plane.kDefault,
                m_Points = new List<float2>(_points),
            });

            bool doBreak = true;
            while (doBreak)
            {
                bool splited = false;
                for (int i = 0; i < m_Nodes.Count; i++)
                {
                    var node = m_Nodes[i];
                    if (node.m_Iteration >= _maxIteration)
                        continue;

                    if (node.m_Points.Count < _nodeCapacity)
                        continue;

                    Divide(node.m_Points, out var dividePlane, out var frontPoints, out var backPoints);

                    m_Nodes.Add(new BSPTreeNode()
                    {
                        m_Iteration = node.m_Iteration + 1,
                        m_Plane = dividePlane,
                        m_Points = frontPoints,
                        front = true,
                    });

                    m_Nodes.Add(new BSPTreeNode()
                    {
                        m_Iteration = node.m_Iteration + 1,
                        m_Plane = dividePlane,
                        m_Points = backPoints,
                        front = false,
                    });

                    m_Nodes.RemoveAt(i);
                    splited = true;
                    break;
                }

                doBreak = splited;
            }
        }

        public void DrawGizmos()
        {
            int index = 0;
            var matrix = Gizmos.matrix;
            foreach (var node in m_Nodes)
            {
                Gizmos.color = UColor.IndexToColor(index++);
                Gizmos.matrix = matrix;
                foreach (var point in node.m_Points)
                    Gizmos.DrawWireSphere(point.to3xz(), .1f);

                if (node.front)
                {
                    Gizmos.matrix = matrix * Matrix4x4.TRS(node.m_Plane.position.to3xz(),
                        Quaternion.LookRotation(node.m_Plane.normal.to3xz()), Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, (Vector3.right + Vector3.up) * 5f);
                    UGizmos.DrawString(Vector3.zero, node.m_Iteration.ToString());
                }
            }

            Gizmos.matrix = matrix;
        }
    }
}
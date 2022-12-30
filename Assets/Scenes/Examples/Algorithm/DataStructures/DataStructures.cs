using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.DataStructures
{
    using static DataStructures.Constants;
    public class DataStructures : MonoBehaviour
    {
        public static class Constants
        {
            public const int kRandomRadius = 20;
        }
        
        public G2Box m_Boundary = new G2Box(-20,20);

        public int m_NodeCapacity = 4;
        public int m_Iteration = 4;
        public float2[] m_RandomPoints;
        [ExtendButton("Random",nameof(Randomize))]
        public int m_RandomCount = 128;
        
        private KDTree m_KDTree = new KDTree();
        private QuadTree m_Tree = new QuadTree();

        void Randomize()
        {
            m_RandomPoints = new float2[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = URandom.Random2DSphere() * kRandomRadius;
                m_RandomPoints[i] = point;
            }
        }

        
        private void OnDrawGizmos()
        {
            if (m_RandomPoints == null)
                return;

            Gizmos.matrix = Matrix4x4.identity;            
            m_Tree.Construct(m_Boundary,m_NodeCapacity,m_Iteration);
            m_Tree.Insert(m_RandomPoints);
            m_Tree.DrawGizmos();

            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 50f);
            m_KDTree.Construct(m_Boundary,m_NodeCapacity,m_Iteration);
            m_KDTree.Insert(m_RandomPoints);
            m_KDTree.DrawGizmos();
        }
    }

    public class TreeNode
    {
        public int m_Iteration;
        public G2Box m_Boundary;
        public List<float2> m_Points;
    }

    public abstract class ATree2D
    {
        private int m_MaxIteration;
        private int m_NodeCapacity;

        private List<TreeNode> m_Nodes = new List<TreeNode>();
        public void Construct(G2Box _boundary, int _nodeCapacity,int _maxIteration)
        {
            m_NodeCapacity = _nodeCapacity;
            m_MaxIteration = _maxIteration;
            m_Nodes.Clear();
            m_Nodes.Add(new TreeNode()
            {
                m_Iteration = 0,
                m_Boundary = _boundary,
                m_Points = new List<float2>(),
            });
        }
        
        public void Insert(float2[] _points)
        {
            foreach (var point in _points)
                Insert(point);
        }

        public void Insert(float2 _point)
        {
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                var node = m_Nodes[i];
                if (!node.m_Boundary.Contains(_point))
                    continue;
                node.m_Points.Add(_point);
                
                if (node.m_Points.Count <= m_NodeCapacity)      //Capacity Check
                    continue;
                
                if (node.m_Iteration >= m_MaxIteration) //Divide
                    continue;
               
                var newIteration = node.m_Iteration+1;
                m_Nodes.AddRange(Divide(node.m_Boundary,node.m_Iteration).Select(_boundary=>new TreeNode()
                {
                    m_Iteration = newIteration,
                    m_Boundary = _boundary,
                    m_Points = node.m_Points.Collect(p=>_boundary.Contains(p)).ToList(),
                }));
                
                m_Nodes.RemoveAt(i);
                i--;
            }
        }

        protected abstract G2Box[] Divide(G2Box _src, int _iteration);
        
        public void DrawGizmos()
        {
            int index = 0;
            foreach (var node in m_Nodes)
            {
                Gizmos.color = UColor.IndexToColor(index++ % 6);
                Gizmos.DrawWireCube(node.m_Boundary.center.to3xz(),node.m_Boundary.size.to3xz()*.99f);
                foreach (var point in node.m_Points)
                    Gizmos.DrawWireSphere(point.to3xz(),.1f);
            }
        }
    }
    
    public class QuadTree : ATree2D
    {
        private static G2Box[] kDivides = new G2Box[4];
        protected override G2Box[] Divide(G2Box _src, int _iteration)
        {
            var b = _src;
            var halfExtent = b.extend / 2;
            var extent = b.size / 2;
            var min = b.min;
            
            kDivides[0]= new G2Box(min + extent * new float2(.5f,.5f),halfExtent);
            kDivides[1]= new G2Box(min + extent * new float2(1.5f,0.5f),halfExtent);
            kDivides[2]= new G2Box(min + extent * new float2(0.5f,1.5f),halfExtent);
            kDivides[3]= new G2Box(min + extent * new float2(1.5f,1.5f),halfExtent);
            return kDivides;
        }
    }
    
    public class KDTree : ATree2D
    {
        private static G2Box[] kDivides = new G2Box[4];
        protected override G2Box[] Divide(G2Box _src, int _iteration)
        {
            var b = _src;
            var extent = b.size / 2;
            var min = b.min;
            
            if (_iteration%2 == 1)
            {
                var halfExtent = b.extend  * new float2(1,0.5f);
                kDivides[0]= new G2Box(min + extent * new float2(1f,.5f),halfExtent);
                kDivides[1]= new G2Box(min + extent * new float2(1f,1.5f),halfExtent);
            }
            else
            {
                var halfExtent = b.extend * new float2(0.5f,1f);
                kDivides[0]= new G2Box(min + extent * new float2(.5f,1f),halfExtent);
                kDivides[1]= new G2Box(min + extent * new float2(1.5f,1f),halfExtent);
            }
            return kDivides;
        }
    }
}
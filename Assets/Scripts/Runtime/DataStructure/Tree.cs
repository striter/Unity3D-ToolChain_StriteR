using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;


    public class TreeNode
    {
        public int m_Iteration;
        public G2Box m_Boundary;
        public List<float2> m_Points;
    }

    public abstract class ATree2D:IEnumerable<G2Box>
    {
        private int m_MaxIteration;
        private int m_NodeCapacity;

        private List<TreeNode> m_Nodes = new List<TreeNode>();
        public int Count => m_Nodes.Count;
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

        public IEnumerator<G2Box> GetEnumerator()
        {
            foreach (var node in m_Nodes)
                yield return node.m_Boundary;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public class QuadTree : ATree2D
    {
        private G2Box[] kDivides;
        private int m_Division = 2;
        
        public void Construct(G2Box _boundary, int _nodeCapacity,int _maxIteration,int _division = 2)
        {
            m_Division = _division;
            kDivides = new G2Box[m_Division * m_Division];
            base.Construct(_boundary,_nodeCapacity,_maxIteration);
        }
        
        protected override G2Box[] Divide(G2Box _src, int _iteration)
        {
            var b = _src;
            var extent = b.extent / m_Division;
            var size = b.size / m_Division;
            var min = b.min;
            for (int i = 0; i < m_Division; i++)
                for (int j = 0; j < m_Division; j++)
                    kDivides[i * m_Division + j] = new G2Box(min + size * new float2( (.5f + i), (.5f + j)), extent);
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
                var halfExtent = b.extent  * new float2(1,0.5f);
                kDivides[0]= new G2Box(min + extent * new float2(1f,.5f),halfExtent);
                kDivides[1]= new G2Box(min + extent * new float2(1f,1.5f),halfExtent);
            }
            else
            {
                var halfExtent = b.extent * new float2(0.5f,1f);
                kDivides[0]= new G2Box(min + extent * new float2(.5f,1f),halfExtent);
                kDivides[1]= new G2Box(min + extent * new float2(1.5f,1f),halfExtent);
            }
            return kDivides;
        }
    }
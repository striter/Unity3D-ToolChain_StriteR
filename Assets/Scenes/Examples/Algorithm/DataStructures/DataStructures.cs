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
        
        // public KDTree m_KDTree;
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
            
            m_Tree.Construct(m_Boundary,m_NodeCapacity,m_Iteration);
            m_Tree.Insert(m_RandomPoints);
            m_Tree.DrawGizmos();
            // m_KDTree.Tick(m_MaxBounds);
            // m_KDTree.DrawGizmos();
        }
    }

    public class QuadTree
    {
        private class QuadTreeNode
        {
            public int m_Iteration;
            public G2Box m_Boundary;
            public List<float2> m_Points;
        }

        private int m_MaxIteration;
        private int m_NodeCapacity;
        private List<QuadTreeNode> m_Nodes = new List<QuadTreeNode>();
        public void Construct(G2Box _boundary, int _nodeCapacity,int _maxIteration)
        {
            m_NodeCapacity = _nodeCapacity;
            m_MaxIteration = _maxIteration;
            m_Nodes.Clear();
            m_Nodes.Add(new QuadTreeNode()
            {
                m_Iteration = 0,
                m_Boundary = _boundary,
                m_Points = new List<float2>(),
            });
        }

        private static G2Box[] kDivides = new G2Box[4];
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
               
                
                var b = node.m_Boundary;
                var halfExtent = b.extend / 2;
                var extent = b.size / 2;
                var min = b.min;
                var newIteration = node.m_Iteration+1;
                
                kDivides[0]= new G2Box(min + extent * new float2(.5f,.5f),halfExtent);
                kDivides[1]= new G2Box(min + extent * new float2(1.5f,0.5f),halfExtent);
                kDivides[2]= new G2Box(min + extent * new float2(0.5f,1.5f),halfExtent);
                kDivides[3]= new G2Box(min + extent * new float2(1.5f,1.5f),halfExtent);

                m_Nodes.RemoveAt(i);
                m_Nodes.AddRange(kDivides.Select(p=>new QuadTreeNode()
                {
                    m_Iteration = newIteration,
                    m_Boundary = p,
                    m_Points = new List<float2>()
                }));
                
                foreach (var point in node.m_Points)
                {
                    var index = kDivides.FindIndex(p => p.Contains(point));
                    if(index==-1)
                        continue;
                    m_Nodes[i+index].m_Points.Add(point);
                }
                
                i--;
            }
        }
        
        public void Insert(float2[] _points)
        {
            foreach (var point in _points)
                Insert(point);
        }

        public void DrawGizmos()
        {
            int index = 0;
            foreach (var node in m_Nodes)
            {
                Gizmos.color = UColor.IndexToColor(index++ % 6);
                Gizmos.DrawWireCube(node.m_Boundary.center.to3xz(),node.m_Boundary.size.to3xz());
                foreach (var point in node.m_Points)
                    Gizmos.DrawWireSphere(point.to3xz(),.1f);
            }
        }
    }
    
    // public enum EAxis
    // {
    //     X,
    //     Y,
    //     Z,
    // }
    //
    // [Serializable]
    // public class KDTree
    // {
    //     [Range(0,1)] public float m_Ratio;
    //     [Clamp(1,20)] public uint iteration = 5;
    //     
    //     private List<Node> nodes = new List<Node>();
    //
    //     public void Tick(GBox _bounds)
    //     {
    //         nodes.Clear();
    //         EAxis axis = EAxis.X;
    //         GBox curBounds = _bounds;
    //         for (int i = 0; i < iteration; i++)
    //         {
    //             float3 curSize = 0;
    //             float3 nextSize = 0;
    //             float3 nextMove = 0;
    //             switch (axis)
    //             {
    //                 case EAxis.X:
    //                 {
    //                     curSize = new float3(m_Ratio, 1, 1);
    //                     nextSize = new float3(1 - m_Ratio, 1, 1);
    //                     nextMove = new float3(m_Ratio, 0, 0);
    //                 }
    //                     break;
    //
    //                 case EAxis.Y:
    //                 {
    //                     curSize = new float3(1, m_Ratio, 1);
    //                     nextSize = new float3(1, 1 - m_Ratio, 1);
    //                     nextMove = new float3(0,  m_Ratio, 0);
    //                 }
    //                     break;
    //                 case EAxis.Z:
    //                 {
    //                     curSize = new float3(1, 1, m_Ratio);
    //                     nextSize = new float3(1, 1, 1 - m_Ratio);
    //                     nextMove =  new float3(0, 0, m_Ratio);
    //                  }
    //                 break;
    //             }
    //
    //             axis = axis.Next();
    //             
    //             nodes.Add(new Node(){bounds = curBounds.Split(0,curSize)});
    //             curBounds = curBounds.Split(nextMove,nextSize);
    //         }
    //         nodes.Add(new Node(){bounds = curBounds});
    //     }
    //
    //     public void DrawGizmos()
    //     {
    //         int index = 0;
    //         foreach (var node in nodes)
    //         {
    //             Gizmos.color = UColor.IndexToColor(index++%6);
    //             node.bounds.DrawGizmos();
    //         }
    //     }
    // }
}
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

        public int m_QuadDivision = 2;
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
            m_Tree.Construct(m_Boundary,m_NodeCapacity,m_Iteration,m_QuadDivision);
            m_Tree.Insert(m_RandomPoints);
            m_Tree.DrawGizmos();

            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 50f);
            m_KDTree.Construct(m_Boundary,m_NodeCapacity,m_Iteration);
            m_KDTree.Insert(m_RandomPoints);
            m_KDTree.DrawGizmos();
        }
    }

}
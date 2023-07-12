using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

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
        public int m_RandomCount = 128;
        
        private KDTree m_KDTree = new KDTree();
        private QuadTree m_QuadTree = new QuadTree();
        private BSPTree m_BSPTree = new BSPTree();


        [FormerlySerializedAs("m_Triangles")] public G2Triangle[] m_RandomTriangles;
        private BVH m_BVH = new BVH();

        [Button]
        void Randomize()
        {
            m_RandomPoints = new float2[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = URandom.Random2DSphere() * kRandomRadius;
                m_RandomPoints[i] = point;
            }

            m_RandomTriangles = new G2Triangle[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
            {
                var point1 = URandom.Random2DSphere() * kRandomRadius;
                var point2 = point1 + URandom.Random2DSphere() * 2f;
                var point3 = point2 + URandom.Random2DSphere() * 2f;
                m_RandomTriangles[i] = new G2Triangle(point1, point2, point3);
            }
        }

        
        private void OnDrawGizmos()
        {
            if (m_RandomPoints == null)
                return;

            Gizmos.matrix = Matrix4x4.identity;            
            m_QuadTree.Construct(m_Boundary,m_NodeCapacity,m_Iteration,m_QuadDivision);
            m_QuadTree.Insert(m_RandomPoints);
            m_QuadTree.DrawGizmos();

            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 50f);
            m_KDTree.Construct(m_Boundary,m_NodeCapacity,m_Iteration);
            m_KDTree.Insert(m_RandomPoints);
            m_KDTree.DrawGizmos();

            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 100f);
            m_BSPTree.Construct(m_RandomPoints,m_Iteration,m_NodeCapacity);
            m_BSPTree.DrawGizmos();

            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 50f);
            m_BVH.Construct(m_RandomTriangles, m_Iteration, m_NodeCapacity);
            m_BVH.DrawGizmos();
        }
    }

}
using Runtime.DataStructure;
using Runtime.Geometry;
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
            public const int kQuadDivision = 2;
        }
        
        [Min(8)]public int m_NodeCapacity = 4;
        public int m_Iteration = 4;
        public int m_RandomCount = 128;
        public float2[] m_RandomPoints;
        
        public G2Triangle[] m_RandomTriangles;
        private QuadTree_float2 m_QuadTree = new QuadTree_float2(kQuadDivision);
        private KDTree_float2 m_KDTree = new KDTree_float2();
        private BSPTree m_BSPTree = new BSPTree();
        private BoundingVolumeHierarchy<TreeNode_triangle2, G2Box, G2Triangle> m_BVH = new();


        public bool m_ParentGizmos;
        public float3[] m_RandomPoints3;
        public GTriangle[] m_RandomTriangles3;
        public QuadTree_float3 m_QuadTree3 = new QuadTree_float3(2);
        public KDTree_float3 m_KDTree3 = new KDTree_float3();
        public QuadTree_triangle3 m_QuadTree_triangle3 = new QuadTree_triangle3(2);
        [Button]
        void Randomize()
        {
            m_RandomPoints = new float2[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
                m_RandomPoints[i] = URandom.Random2DSphere() * kRandomRadius;

            m_RandomTriangles = new G2Triangle[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
            {
                var point1 = URandom.Random2DSphere() * kRandomRadius;
                var point2 = point1 + URandom.Random2DSphere() * 2f;
                var point3 = point2 + URandom.Random2DSphere() * 2f;
                m_RandomTriangles[i] = new G2Triangle(point1, point2, point3);
            }
            
            m_RandomPoints3 = new float3[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
                m_RandomPoints3[i] =  URandom.RandomSphere() * kRandomRadius;
            
            m_RandomTriangles3 = new GTriangle[m_RandomCount];
            for (int i = 0; i < m_RandomCount; i++)
            {
                var point1 = URandom.RandomSphere() * kRandomRadius;
                var point2 = point1 + URandom.RandomSphere() * 2f;
                var point3 = point2 + URandom.RandomSphere() * 2f;
                m_RandomTriangles3[i] = new GTriangle(point1, point2, point3);
            }
        }

        
        private void OnDrawGizmos()
        {
            if (m_RandomPoints == null)
                return;

            Gizmos.matrix = Matrix4x4.identity;            
            m_QuadTree.Construct(m_RandomPoints, m_Iteration,m_NodeCapacity);
            m_QuadTree.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 50f);
            m_KDTree.Construct(m_RandomPoints,m_Iteration,m_NodeCapacity);
            m_KDTree.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 100f);
            m_BSPTree.Construct(m_RandomPoints,m_Iteration,m_NodeCapacity);
            m_BSPTree.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 150f);
            m_BVH.Construct(m_RandomTriangles, m_Iteration,m_NodeCapacity);
            m_BVH.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 50f);
            m_QuadTree3.Construct(m_RandomPoints3,m_Iteration,m_NodeCapacity);
            m_QuadTree3.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 50f + Vector3.right*50f);
            m_KDTree3.Construct(m_RandomPoints3,m_Iteration,m_NodeCapacity);
            m_KDTree3.DrawGizmos(m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 100f);
            m_QuadTree_triangle3.Construct(m_RandomTriangles3,m_Iteration,m_NodeCapacity);
            m_QuadTree_triangle3.DrawGizmos(m_ParentGizmos);
        }
    }

}
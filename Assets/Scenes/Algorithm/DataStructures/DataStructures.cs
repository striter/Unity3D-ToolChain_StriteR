using Runtime.DataStructure;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.DataStructures
{
    using static DataStructures.Constants;
    using static BoundaryTreeHelper;
    public class DataStructures : MonoBehaviour
    {
        public static class Constants
        {
            public const int kRandomRadius = 20;
            public const int kQuadDivision = 2;
            public const int kNodeCapacity = 8;
            public const int kMaxIteration = 4;
        }
        
        public int m_RandomCount = 128;
        public float2[] m_RandomPoints;
        
        public G2Triangle[] m_RandomTriangles;
        private QuadTree2<float2,G2Box_float2> m_QuadTree = new (kNodeCapacity,kMaxIteration,kQuadDivision);
        private KDTree<G2Box,float2,G2Box_float2> m_KDTree = new (kNodeCapacity,kMaxIteration);
        private BSPTree m_BSPTree = new(kNodeCapacity,kMaxIteration);
        private BoundingVolumeHierarchy<G2Box, G2Triangle , G2Box_G2Triangle> m_BVH = new(kNodeCapacity,kMaxIteration);

        public bool m_ParentGizmos;
        public float3[] m_RandomPoints3;
        public GTriangle[] m_RandomTriangles3;
        public QuadTree3<float3,GBox_float3> m_QuadTree3 = new (kNodeCapacity,kMaxIteration,kQuadDivision);
        public KDTree<GBox,float3,GBox_float3> m_KDTree3 = new (kNodeCapacity,kMaxIteration);
        public QuadTree3<GTriangle,GBox_GTriangle> m_QuadTree_triangle3 = new (kNodeCapacity,kMaxIteration,kQuadDivision);
        [InspectorButton]
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
            m_QuadTree.Construct(m_RandomPoints);
            m_QuadTree.DrawGizmos(m_RandomPoints,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 50f);
            m_KDTree.Construct(m_RandomPoints);
            m_KDTree.DrawGizmos(m_RandomPoints,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 100f);
            m_BSPTree.Construct(m_RandomPoints);
            m_BSPTree.DrawGizmos(m_RandomPoints,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.right * 150f);
            m_BVH.Construct(m_RandomTriangles);
            m_BVH.DrawGizmos(m_RandomTriangles,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 50f);
            m_QuadTree3.Construct(m_RandomPoints3);
            m_QuadTree3.DrawGizmos(m_RandomPoints3,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 50f + Vector3.right*50f);
            m_KDTree3.Construct(m_RandomPoints3);
            m_KDTree3.DrawGizmos(m_RandomPoints3,m_ParentGizmos);
            
            Gizmos.matrix = Matrix4x4.Translate(Vector3.back * 100f);
            m_QuadTree_triangle3.Construct(m_RandomTriangles3);
            m_QuadTree_triangle3.DrawGizmos(m_RandomTriangles3,m_ParentGizmos);
        }
    }

}
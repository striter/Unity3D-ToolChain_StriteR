using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.GeometryVisualize
{
    using static DelaunayTriangulationSphereVisualize.Constants;
    [ExecuteInEditMode]
    public class DelaunayTriangulationSphereVisualize : MonoBehaviour
    {        
        public static class Constants
        {
            public const float kSphereRadius = 10f;
        }
        public int m_RandomCount = 128;
        public List<float3> m_Vertices = new List<float3>();

        [InspectorButton(true)]
        void Randomize()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
                m_Vertices.Add(URandom.RandomDirection() * kSphereRadius);
        }

        [InspectorButton(true)]
        void Fibonacci()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
                m_Vertices.Add(USphereMapping.LowDiscrepancySequences.Fibonacci(i,m_RandomCount)*kSphereRadius);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var point in m_Vertices)
                Gizmos.DrawSphere(point,.1f);
            UGeometry.BowyerWatson_Spherical(m_Vertices,out var triangles);
            foreach (var triangle in triangles)
                UGizmos.DrawLinesConcat(triangle,_p=>m_Vertices[_p]);
            Gizmos.color = KColor.kOrange.SetA(.3f);
            foreach (var point in UGeometry.kProjectedVertices)
                Gizmos.DrawWireSphere(point.to3xz(-kSphereRadius),.3f);
        }
    }

}
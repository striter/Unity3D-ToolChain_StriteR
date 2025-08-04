using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.GeometryVisualize
{
    
    public class DelaunayTetrahedron : MonoBehaviour
    {
        public List<float3> m_Vertices = new List<float3>();
        private List<PTriangle> tetrahedrons = new List<PTriangle>();

        [InspectorButton(true)]
        void Randomize()
        {
            for (int i = 0; i < m_Vertices.Count; i++)
                m_Vertices[i] = URandom.RandomSphere() * 10f;
        }

        [InspectorButton(true)]
        void Fibonacci()
        {
            for (int i = 0; i < m_Vertices.Count; i++)
                m_Vertices[i]= USphereMapping.LowDiscrepancySequences.Fibonacci(i,m_Vertices.Count)*10f;
        }
        
        [InspectorButton(true)]
        void Sequence()
        {
            for (int i = 0; i < m_Vertices.Count; i++)
                m_Vertices[i]= (ULowDiscrepancySequences.Hammersley3D((uint)i,(uint)m_Vertices.Count) - .5f)*10f;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            foreach (var vertex in m_Vertices)
                Gizmos.DrawWireSphere(vertex, .1f);
            UTriangulation.Triangulation(m_Vertices,ref tetrahedrons);
            foreach (var tetrahedron in tetrahedrons)
                new GTriangle(m_Vertices,tetrahedron).DrawGizmos();
        }
    }

}
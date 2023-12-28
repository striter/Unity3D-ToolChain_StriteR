using System.Collections.Generic;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;
using UGeometry = Geometry.Validation.UGeometry;

namespace Examples.Algorithm.DelaunayTriangulation
{
    using static DelaunayTriangulationSphere.Constants;
    [ExecuteInEditMode]
    public class DelaunayTriangulationSphere : MonoBehaviour
    {        
        public static class Constants
        {
            public const float kSphereRadius = 10f;
        }
        public int m_RandomCount = 128;
        public List<float3> m_Vertices = new List<float3>();
        private List<float2> m_ProjectedVertices = new List<float2>();
        private List<PTriangle> triangles = new List<PTriangle>();

        [Button]
        void Randomize()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
                m_Vertices.Add(URandom.RandomDirection() * kSphereRadius);
            OnValidate();
        }

        [Button]
        void Fibonacci()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
                m_Vertices.Add(ULowDiscrepancySequences.FibonacciSphere(i,m_RandomCount)*kSphereRadius);
            OnValidate();
        }

        void PoleTriangulation(float3 _poleOrigin,GPlane _projectionPlane,ref List<PTriangle> _triangles)
        {
            List<PTriangle> curTriangles = new List<PTriangle>();
            
            m_ProjectedVertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
                m_ProjectedVertices.Add(UGeometry.Projection(_projectionPlane,m_Vertices[i],_poleOrigin).xz);
            
            Triangulation.BowyerWatson(m_ProjectedVertices,ref curTriangles);
            _triangles.AddRange(curTriangles);
        }
        
        private void OnValidate()
        {
            triangles.Clear();
            PoleTriangulation(new float3(0,kSphereRadius,0),new GPlane(Vector3.up,-kSphereRadius ),ref triangles);      //Project from north pole
            PoleTriangulation(new float3(0,-kSphereRadius,0),new GPlane(Vector3.down,-kSphereRadius ),ref triangles);       //Project from south pole
            
            for (int i = 0; i < triangles.Count; i++)       //Exclude abundant triangles
            {
                var cur = triangles[i];
                for(int j=0;j<triangles.Count;j++)
                    if (j != i && cur.MatchVertexCount(triangles[j]) == 3)
                    {
                        triangles.RemoveAt(i);
                        break;
                    }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var point in m_Vertices)
                Gizmos.DrawSphere(point,.1f);
            foreach (var triangle in triangles)
                UGizmos.DrawLinesConcat(triangle,_p=>m_Vertices[_p]);
            
            Gizmos.color = KColor.kOrange.SetA(.3f);
            foreach (var point in m_ProjectedVertices)
                Gizmos.DrawWireSphere(point.to3xz(-kSphereRadius),.3f);
        }
    }

}
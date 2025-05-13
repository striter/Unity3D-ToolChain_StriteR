using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.DelaunayTriangulation
{
    using static DelaunayTriangulation2D.Constants;
    [ExecuteInEditMode]
    public class DelaunayTriangulation2D : MonoBehaviour
    {
        public static class Constants
        {
            public const float kRandomRadius = 10f;
        }
        public uint m_RandomCount = 128;
        public List<float2> m_Vertices = new List<float2>();
        private List<PTriangle> triangles = new List<PTriangle>();

        [InspectorButton(true)]
        void Randomize()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = URandom.Random2DSphere() * kRandomRadius;
                m_Vertices.Add(point);
            }
            OnValidate();
        }

        [InspectorButton(true)]
        void Sequence()
        {
            m_Vertices.Clear();
            for (uint i = 0; i < m_RandomCount; i++)
            {
                var p = ULowDiscrepancySequences.Hammersley2D(i, m_RandomCount)  - .5f;
                math.sincos(p.x*math.PI*2,out var s,out var c);
                m_Vertices.Add(new float2(s,c)*p.y*kRandomRadius);
            }
            OnValidate();
        }
        
        [InspectorButton(true)]
        void PoissonDisk()
        {
            var size = (int)math.sqrt(m_RandomCount);
            ULowDiscrepancySequences.PoissonDisk2D(size*size).Select(p=>(p - .5f)*kRandomRadius).FillList(m_Vertices);
            OnValidate();
        }
        private void OnValidate()
        {
            triangles.Clear();
            UTriangulation.BowyerWatson(m_Vertices,ref triangles);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one);
            Gizmos.color = Color.white;
            foreach (var point in m_Vertices)
                Gizmos.DrawWireSphere(point.to3xz(),.1f);
            Gizmos.color = Color.white.SetA(.1f);
            foreach (var triangle in triangles)
                UGizmos.DrawLinesConcat(triangle,_p=>m_Vertices[_p].to3xz());
        }

    }
}

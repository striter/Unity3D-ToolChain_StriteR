using System.Collections.Generic;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.DelaunayTrianglulation
{
    using static DDelaunay;
    public static class DDelaunay
    {
        public const float kRandomRadius = 10f;
    }
    [ExecuteInEditMode]
    public class DelaunayTriangulation : MonoBehaviour
    {
        [FormerlySerializedAs("randomCount")]
        [ExtendButton("Randomize",nameof(Randomize),null,
            "Sequence",nameof(Sequence),null)]
        public uint m_RandomCount = 128;
        [FormerlySerializedAs("vertices")] public List<float2> m_Vertices = new List<float2>();
        private List<PTriangle> triangles = new List<PTriangle>();

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

        void Sequence()
        {
            m_Vertices.Clear();
            var ldsPoints = ULowDiscrepancySequences.Hammersley2D(m_RandomCount,0f);
            for (int i = 0; i < m_RandomCount; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = ldsPoints[i];
                math.sincos(point.x*math.PI*2,out var s,out var c);
                m_Vertices.Add(new float2(s,c)*point.y*kRandomRadius);
            }
            OnValidate();
        }
        
        private void OnValidate()
        {
            triangles.Clear();
            UTriangulation.BowyerWatson(m_Vertices,ref triangles);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white.SetAlpha(.1f);
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one.SetY(0f));
            Gizmos.color = Color.white.SetAlpha(.3f);
            Gizmos.matrix = Matrix4x4.identity;;
            foreach (var point in m_Vertices)
                Gizmos.DrawWireSphere(point.to3xz(),.1f);
            foreach (var triangle in triangles)
                Gizmos_Extend.DrawLinesConcat(triangle,_p=>m_Vertices[_p].to3xz());
        }

    }
}

using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.DelaunayTrianglulation
{
    using static DelaunayTriangulation.Constants;
    [ExecuteInEditMode]
    public class DelaunayTriangulation : MonoBehaviour
    {
        public static class Constants
        {
            public const float kRandomRadius = 10f;
        }
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
            ULowDiscrepancySequences.Hammersley2D(m_RandomCount,0f).Select(p => {
                math.sincos(p.x*math.PI*2,out var s,out var c);
                return new float2(s,c)*p.y*kRandomRadius;
            }).FillList(m_Vertices);
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

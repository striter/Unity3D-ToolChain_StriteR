using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Delaunay
{
    using static DDelaunay;
    public static class DDelaunay
    {
        public const int kPointsAmount = 128;
        public const float kRandomRadius = 10f;
    }
    [ExecuteInEditMode]
    public class DelaunayTriangulation : MonoBehaviour
    {
        float2[] vertices;
        private List<PTriangle> triangles = new List<PTriangle>();

        private void OnValidate()
        {
            var ldsPoints = ULowDiscrepancySequences.Hammersley2D(kPointsAmount,0f);
            
            vertices = new float2[kPointsAmount];
            for (int i = 0; i < vertices.Length; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = ldsPoints[i];
                math.sincos(point.x*math.PI*2,out var s,out var c);
                vertices[i] = new float2(s,c)*point.y*kRandomRadius;
            }
            
            triangles.Clear();
            UTriangulation.BowyerWatson(vertices,ref triangles);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white.SetAlpha(.1f);
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one.SetY(0f));
            Gizmos.color = Color.white.SetAlpha(.3f);
            Gizmos.matrix=Matrix4x4.identity;;
            foreach (var point in vertices)
                Gizmos.DrawWireSphere(point.to3xz(),.1f);
            foreach (var triangle in triangles)
                Gizmos_Extend.DrawLinesConcat(triangle,p=>vertices[p].to3xz());
        }

    }
}

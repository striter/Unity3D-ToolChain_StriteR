using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static partial class UTriangulation
    {
        private static List<PTriangle> kSphericalTriangles = new List<PTriangle>();
        public static List<float2> kProjectedVertices = new List<float2>();
        private static List<PTriangle> kTempTriangles = new List<PTriangle>();
        static void PoleTriangulation(IList<float3> _vertices, float3 _poleOrigin,GPlane _projectionPlane,ref List<PTriangle> _triangles)
        {
            kProjectedVertices.Clear();
            for (int i = 0; i < _vertices.Count; i++)
                kProjectedVertices.Add(_projectionPlane.Projection(_vertices[i],_poleOrigin).xz);
            
            Triangulation(kProjectedVertices,ref kTempTriangles);
            _triangles.AddRange(kTempTriangles);
        }
        
        public static void BowyerWatson_Spherical(IList<float3> _vertices,out List<PTriangle> _triangles)
        {
            _triangles = kSphericalTriangles;
            
            _triangles.Clear();
            var sphere = GSphere.GetBoundingSphere(_vertices);
            var kSphereRadius = sphere.radius;
            PoleTriangulation(_vertices, sphere.Origin + new float3(0,kSphereRadius,0),new GPlane(kfloat3.up, sphere.Origin - kfloat3.up*kSphereRadius ),ref _triangles);      //Project from north pole
            PoleTriangulation(_vertices, sphere.Origin + new float3(0,-kSphereRadius,0),new GPlane(kfloat3.down, sphere.Origin - kfloat3.down*kSphereRadius ),ref _triangles);       //Project from south pole
            for (int i = 0; i < _triangles.Count; i++)       //Exclude abundant triangles
            {
                var cur = _triangles[i];
                for(int j=0;j<_triangles.Count;j++)
                    if (j != i && cur.MatchVertexCount(_triangles[j]) == 3)
                    {
                        _triangles.RemoveAt(i);
                        break;
                    }
            }

        }
    }

}

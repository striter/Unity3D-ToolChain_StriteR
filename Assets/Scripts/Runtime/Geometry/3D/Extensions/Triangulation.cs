using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        private static List<PTriangle> kSphericalTriangles = new List<PTriangle>();
        private static List<PTriangle> kTempTriangles = new List<PTriangle>();
        public static List<float2> kProjectedVertices = new List<float2>();

        static void PoleTriangulation(IList<float3> _vertices, float3 _poleOrigin, GPlane _projectionPlane,
            ref List<PTriangle> _triangles)
        {
            kProjectedVertices.Clear();
            for (int i = 0; i < _vertices.Count; i++)
                kProjectedVertices.Add(_projectionPlane.Projection(_vertices[i], _poleOrigin).xz);

            Triangulation(kProjectedVertices, ref kTempTriangles);
            _triangles.AddRange(kTempTriangles);
        }

        public static void BowyerWatson_Spherical(IList<float3> _vertices, out List<PTriangle> _triangles)
        {
            _triangles = kSphericalTriangles;

            _triangles.Clear();
            var sphere = GSphere.GetBoundingSphere(_vertices);
            var kSphereRadius = sphere.radius;
            PoleTriangulation(_vertices, sphere.Origin + new float3(0, kSphereRadius, 0),
                new GPlane(kfloat3.up, sphere.Origin - kfloat3.up * kSphereRadius),
                ref _triangles); //Project from north pole
            PoleTriangulation(_vertices, sphere.Origin + new float3(0, -kSphereRadius, 0),
                new GPlane(kfloat3.down, sphere.Origin - kfloat3.down * kSphereRadius),
                ref _triangles); //Project from south pole
            for (int i = 0; i < _triangles.Count; i++) //Exclude abundant triangles
            {
                var cur = _triangles[i];
                for (int j = 0; j < _triangles.Count; j++)
                    if (j != i && cur.MatchVertexCount(_triangles[j]) == 3)
                    {
                        _triangles.RemoveAt(i);
                        break;
                    }
            }
        }
        
        public static class DelaunayTetrahedron
        {
            private struct DComplex : IEquatable<DComplex>
            {
                public PTetrahedron complex;
                public GSphere circumscribedSphere;
        
                public bool Equals(DComplex other)
                {
                    return complex.Equals(other.complex);
                }
        
                public override bool Equals(object obj)
                {
                    return obj is DComplex other && Equals(other);
                }
        
                public override int GetHashCode()
                {
                    return complex.GetHashCode();
                }
            }
        
            private static List<float3> kVertices = new();
            private static List<DComplex> kComplexes = new();
            private static List<PTriangle> kPolygon = new();
            public static void BowyerWatson(IList<float3> _vertices, ref List<PTriangle> _triangles)
            {
                kVertices.Clear();
                kComplexes.Clear();
                kPolygon.Clear();
                var vertexCount = _vertices.Count;
                kVertices.AddRange(_vertices);
                var superTetrahedron = GTetrahedron.GetSuperTetrahedron(GBox.GetBoundingBox(_vertices));
                kVertices.AddRange(superTetrahedron);
                kComplexes.Add(new DComplex()
                {
                    complex = new PTetrahedron(0,1,2,3) + vertexCount,
                    circumscribedSphere = GSphere.Tetrahedron(superTetrahedron.v0,superTetrahedron.v1,superTetrahedron.v2,superTetrahedron.v3)
                });
                
                for(var vertexIndex=0;vertexIndex<vertexCount;vertexIndex++)
                {
                    var vertex = _vertices[vertexIndex];
                    kPolygon.Clear();
                    
                    for(var i=0;i<kComplexes.Count;i++)
                    {
                        var complex = kComplexes[i];
                        if (!complex.circumscribedSphere.Contains(vertex))
                            continue;
                        var polygon = complex.complex;
                        foreach (var triangle in polygon.GetTriangles())
                            kPolygon.Add(triangle.Distinct());
                        kComplexes.RemoveAt(i);
                        i--;
                    }
                    
                    foreach (var polygon in kPolygon)
                    {
                        if (kPolygon.Count(p => p.Equals(polygon)) > 1)
                            continue;
                        
                        var tetrahedron = new PTetrahedron(polygon.V0,polygon.V1,polygon.V2,vertexIndex);
                        var positions = new GTetrahedron(kVertices,tetrahedron);
                        kComplexes.Insert(0,new DComplex(){complex = tetrahedron,circumscribedSphere = GSphere.Tetrahedron(positions.v0,positions.v1,positions.v2,positions.v3)});
                    }
                }
                
                //Remove triangles shared edge with super triangle
                _triangles.Clear();
                for (var i = kComplexes.Count - 1; i >= 0; i--)
                {
                    foreach (var triangle in kComplexes[i].complex.GetTriangles())
                    {
                        if(!triangle.Any(p=>p>=vertexCount))  //Is from super triangle
                            _triangles.Add(triangle);
                    }
                }
            }
            
        }
        public static void Triangulation(IList<float3> _vertices, ref List<PTriangle> _tetrahedrons) => DelaunayTetrahedron.BowyerWatson(_vertices, ref _tetrahedrons);
    }

}

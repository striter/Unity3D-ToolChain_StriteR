using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static class DelaunayTriangulation
        {
            private struct DComplex : IEquatable<DComplex>
            {
                public PTriangle complex;
                public G2Circle circumscribedCircle;

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

            private static List<float2> kVertices = new();
            private static List<DComplex> kComplexes = new();
            private static List<PLine> kPolygon = new();
            public static void BowyerWatson(IList<float2> _vertices, ref List<PTriangle> _triangles)
            {
                _triangles.Clear();
                kVertices.Clear();
                kComplexes.Clear();
                kPolygon.Clear();
                var vertexCount = _vertices.Count;
                kVertices.AddRange(_vertices);
                var boundsCircle = G2Circle.GetBoundingCircle(_vertices);
                var superTriangle = G2Triangle.GetCircumscribedTriangle(boundsCircle);
                kVertices.AddRange(superTriangle);
                kComplexes.Add(new DComplex()
                {
                    complex = new PTriangle(0,1,2) + vertexCount,
                    circumscribedCircle = G2Circle.TriangleCircumscribed(superTriangle.V0,superTriangle.V1,superTriangle.V2)
                });

                
                for(var vertexIndex=0;vertexIndex<vertexCount;vertexIndex++)
                {
                    var vertex = _vertices[vertexIndex];
                    kPolygon.Clear();
                    
                    for(var i=0;i<kComplexes.Count;i++)
                    {
                        var triangle = kComplexes[i];
                        if (!triangle.circumscribedCircle.Contains(vertex))
                            continue;
                        foreach (var edge in triangle.complex.GetEdges())
                            kPolygon.Add(edge.Distinct());
                        kComplexes.RemoveAt(i);
                        i--;
                    }
                    
                    foreach (var edge in kPolygon)
                    {
                        if (kPolygon.Count(p => p.Equals(edge)) > 1)
                            continue;
                        
                        var polygon = new PTriangle(edge.start,edge.end,vertexIndex);
                        var positions = new G2Triangle(kVertices,polygon);
                        kComplexes.Insert(0,new DComplex(){complex = polygon,circumscribedCircle = G2Circle.TriangleCircumscribed(positions)});
                    }
                }
                
                //Remove triangles shared edge with super triangle
                _triangles.Clear();
                for(var i= kComplexes.Count-1;i>=0;i--)
                    if(!kComplexes[i].complex.Any(p=>p>=vertexCount))  //Is from super triangle
                        _triangles.Add(kComplexes[i].complex);
            }
        }
        
        public static void Triangulation(IList<float2> _vertices,ref List<PTriangle> _triangles) => DelaunayTriangulation.BowyerWatson(_vertices,ref _triangles);
    }
}
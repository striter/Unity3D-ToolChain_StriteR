using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UTriangulation
    {
        public static class DelaunayTriangulation2D
        {
            private struct DComplex : IEquatable<DComplex>
            {
                public PTriangle complex;
                public G2Triangle positions;
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

            private struct DEdge : IEquatable<DEdge>
            {
                public PLine polygon;
                public G2Line positions;

                public DEdge(PLine _polygon,G2Line _positions)
                {
                    polygon = _polygon;
                    positions = _positions;
                }

                public bool Equals(DEdge other)
                {
                    return polygon.EqualsNonVector(other.polygon);
                }

                public override bool Equals(object obj)
                {
                    return obj is DEdge other && Equals(other);
                }

                public override int GetHashCode()
                {
                    return polygon.GetHashCode();
                }
            }
            
            private static List<DComplex> kComplexes = new List<DComplex>();
            private static List<DEdge> kEdges = new List<DEdge>();
            private static PTriangle kSuperComplex = new PTriangle(-1, -2, -3);
            public static void BowyerWatson(IList<float2> _vertices, ref List<PTriangle> _triangles)
            {
                _triangles.Clear();
                kComplexes.Clear();
                kEdges.Clear();
                var boundsCircle = G2Circle.GetBoundingCircle(_vertices);
                var superTriangle = G2Triangle.GetCircumscribedTriangle(boundsCircle);
                kComplexes.Add(new DComplex()
                {
                    complex = kSuperComplex,
                    positions = superTriangle,
                    circumscribedCircle = G2Circle.TriangleCircumscribed(superTriangle.V0,superTriangle.V1,superTriangle.V2)
                });
                
                var vertexIndex = 0;
                foreach (var vertex in _vertices)
                {
                    kEdges.Clear();
                    
                    for(var i=0;i<kComplexes.Count;i++)
                    {
                        var triangle = kComplexes[i];
                        if (!triangle.circumscribedCircle.Contains(vertex))
                            continue;
                        var polygon = triangle.complex;
                        var positions = triangle.positions;
                        kEdges.Add(new DEdge(new PLine(polygon.V0,polygon.V1), new G2Line(positions.V0,positions.V1)));
                        kEdges.Add(new DEdge(new PLine(polygon.V1,polygon.V2), new G2Line(positions.V1,positions.V2)));
                        kEdges.Add(new DEdge(new PLine(polygon.V2,polygon.V0), new G2Line(positions.V2,positions.V0)));
                        kComplexes.RemoveAt(i);
                        i--;
                    }
                    
                    foreach (var edge in kEdges)
                    {
                        if (kEdges.Count(p => p.Equals(edge)) > 1)
                            continue;
                        
                        var polygon = new PTriangle(edge.polygon.start,edge.polygon.end,vertexIndex);
                        var positions = new G2Triangle(edge.positions.start,edge.positions.end,vertex);
                        kComplexes.Insert(0,new DComplex(){complex = polygon,positions = positions,circumscribedCircle = G2Circle.TriangleCircumscribed(positions)});
                    }

                    vertexIndex++;
                }
                
                //Remove triangles shared edge with super triangle
                _triangles.Clear();
                for(var i= kComplexes.Count-1;i>=0;i--)
                    if(!kComplexes[i].complex.Any(p=>p<0))  //Is from super triangle
                        _triangles.Add(kComplexes[i].complex);
            }
        }
        
        public static void Triangulation(IList<float2> _vertices,ref List<PTriangle> _triangles) => DelaunayTriangulation2D.BowyerWatson(_vertices,ref _triangles);
    }
}
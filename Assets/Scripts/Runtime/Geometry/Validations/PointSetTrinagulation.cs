using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Geometry.Validation
{
    public static class Triangulation
    {
        public struct DTriangle : IEquatable<DTriangle>
        {
            public PTriangle polygon;
            public G2Triangle positions;
            public G2Circle circumscribedCircle;

            public bool Equals(DTriangle other)
            {
                return polygon.Equals(other.polygon);
            }

            public override bool Equals(object obj)
            {
                return obj is DTriangle other && Equals(other);
            }

            public override int GetHashCode()
            {
                return polygon.GetHashCode();
            }
        }

        public struct DEdge : IEquatable<DEdge>
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
        
        private static List<DTriangle> kTriangles = new List<DTriangle>();
        private static List<DEdge> kEdges = new List<DEdge>();
        private static PTriangle kSuperPolygon = new PTriangle(-1, -2, -3);
        public static void BowyerWatson(IList<float2> _vertices, ref List<PTriangle> _triangles)
        {
            kTriangles.Clear();
            kEdges.Clear();
            var boundsCircle = UBounds.GetBoundingCircle(_vertices);
            var superTriangle = boundsCircle.GetCircumscribedTriangle();
            kTriangles.Add(new DTriangle()
            {
                polygon = kSuperPolygon,
                positions = superTriangle,
                circumscribedCircle = G2Circle.TriangleCircumscribed(superTriangle.V0,superTriangle.V1,superTriangle.V2)
            });
            
            int vertexIndex = 0;
            foreach (var vertex in _vertices)
            {
                kEdges.Clear();
                
                for(int i=0;i<kTriangles.Count;i++)
                {
                    var triangle = kTriangles[i];
                    if (!triangle.circumscribedCircle.Contains(vertex))
                        continue;
                    var polygon = triangle.polygon;
                    var positions = triangle.positions;
                    kEdges.Add(new DEdge(new PLine(polygon.V0,polygon.V1), new G2Line(positions.V0,positions.V1)));
                    kEdges.Add(new DEdge(new PLine(polygon.V1,polygon.V2), new G2Line(positions.V1,positions.V2)));
                    kEdges.Add(new DEdge(new PLine(polygon.V2,polygon.V0), new G2Line(positions.V2,positions.V0)));
                    kTriangles.RemoveAt(i);
                    i--;
                }
                
                foreach (var edge in kEdges)
                {
                    if (kEdges.Count(p => p.Equals(edge)) > 1)
                        continue;
                    
                    var polygon = new PTriangle(edge.polygon.start,edge.polygon.end,vertexIndex);
                    var positions = new G2Triangle(edge.positions.start,edge.positions.end,vertex);
                    kTriangles.Insert(0,new DTriangle(){polygon = polygon,positions = positions,circumscribedCircle = G2Circle.TriangleCircumscribed(positions)});
                }

                vertexIndex++;
            }
            
            //Remove triangles shared edge with super triangle
            _triangles.Clear();
            for(int i=kTriangles.Count-1;i>=0;i--)
                if(!kTriangles[i].polygon.Any(p=>p<0))  //Is from super triangle
                    _triangles.Add(kTriangles[i].polygon);
        }
    }

}

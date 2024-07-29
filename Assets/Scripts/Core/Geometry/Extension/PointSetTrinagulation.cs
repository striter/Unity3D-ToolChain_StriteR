using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension
{
    public static class UTriangulation
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
            _triangles.Clear();
            kTriangles.Clear();
            kEdges.Clear();
            var boundsCircle = UGeometry.GetBoundingCircle(_vertices);
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

        
        private static List<PTriangle> kSphericalTriangles = new List<PTriangle>();
        public static List<float2> kProjectedVertices = new List<float2>();
        private static List<PTriangle> kTempTriangles = new List<PTriangle>();
        static void PoleTriangulation(IList<float3> _vertices, float3 _poleOrigin,GPlane _projectionPlane,ref List<PTriangle> _triangles)
        {
            kProjectedVertices.Clear();
            for (int i = 0; i < _vertices.Count; i++)
                kProjectedVertices.Add(UGeometry.Projection(_projectionPlane,_vertices[i],_poleOrigin).xz);
            
            BowyerWatson(kProjectedVertices,ref kTempTriangles);
            _triangles.AddRange(kTempTriangles);
        }
        
        public static void BowyerWatson_Spherical(IList<float3> _vertices,out List<PTriangle> _triangles)
        {
            _triangles = kSphericalTriangles;
            
            _triangles.Clear();
            var sphere = UGeometry.GetBoundingSphere(_vertices);
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

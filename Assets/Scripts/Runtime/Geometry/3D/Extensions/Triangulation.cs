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
        private static List<PTriangle> kTempTriangles = new List<PTriangle>();
        public static List<float2> kProjectedVertices = new List<float2>();
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

        // public static class DelaunayTriangulation3D
        // {
        //     private struct DComplex : IEquatable<DComplex>
        //     {
        //         public PTriangle polygon;
        //         public GTriangle positions;
        //         public GSphere circumscribedCircle;
        //
        //         public bool Equals(DComplex other)
        //         {
        //             return polygon.Equals(other.polygon);
        //         }
        //
        //         public override bool Equals(object obj)
        //         {
        //             return obj is DComplex other && Equals(other);
        //         }
        //
        //         public override int GetHashCode()
        //         {
        //             return polygon.GetHashCode();
        //         }
        //     }
        //
        //     private struct DEdge : IEquatable<DEdge>
        //     {
        //         public PLine polygon;
        //         public GLine positions;
        //
        //         public DEdge(PLine _polygon,GLine _positions)
        //         {
        //             polygon = _polygon;
        //             positions = _positions;
        //         }
        //
        //         public bool Equals(DEdge other)
        //         {
        //             return polygon.EqualsNonVector(other.polygon);
        //         }
        //
        //         public override bool Equals(object obj)
        //         {
        //             return obj is DEdge other && Equals(other);
        //         }
        //
        //         public override int GetHashCode()
        //         {
        //             return polygon.GetHashCode();
        //         }
        //     }
        //     
        //     private static List<DComplex> kTriangles = new List<DComplex>();
        //     private static List<DEdge> kEdges = new List<DEdge>();
        //     private static PTriangle kSuperPolygon = new PTriangle(-1, -2, -3);
        //     public static void BowyerWatson(IList<float3> _vertices, ref List<PTriangle> _triangles)
        //     {
        //         _triangles.Clear();
        //         kTriangles.Clear();
        //         kEdges.Clear();
        //         var boundsCircle = GSphere.GetBoundingSphere(_vertices);
        //         var superComplex = GTriangle.GetCircumscribedTriangle(boundsCircle);
        //         kTriangles.Add(new DComplex()
        //         {
        //             polygon = kSuperPolygon,
        //             positions = superComplex,
        //             circumscribedCircle = GSphere.Triangle(superComplex.V0,superComplex.V1,superComplex.V2)
        //         });
        //         
        //         var vertexIndex = 0;
        //         foreach (var vertex in _vertices)
        //         {
        //             kEdges.Clear();
        //             
        //             for(var i=0;i<kTriangles.Count;i++)
        //             {
        //                 var triangle = kTriangles[i];
        //                 if (!triangle.circumscribedCircle.Contains(vertex))
        //                     continue;
        //                 var polygon = triangle.polygon;
        //                 var positions = triangle.positions;
        //                 kEdges.Add(new DEdge(new PLine(polygon.V0,polygon.V1), new GLine(positions.V0,positions.V1)));
        //                 kEdges.Add(new DEdge(new PLine(polygon.V1,polygon.V2), new GLine(positions.V1,positions.V2)));
        //                 kEdges.Add(new DEdge(new PLine(polygon.V2,polygon.V0), new GLine(positions.V2,positions.V0)));
        //                 kTriangles.RemoveAt(i);
        //                 i--;
        //             }
        //             
        //             foreach (var edge in kEdges)
        //             {
        //                 if (kEdges.Count(p => p.Equals(edge)) > 1)
        //                     continue;
        //                 
        //                 var polygon = new PTriangle(edge.polygon.start,edge.polygon.end,vertexIndex);
        //                 var positions = new G2Triangle(edge.positions.start,edge.positions.end,vertex);
        //                 kTriangles.Insert(0,new DComplex(){polygon = polygon,positions = positions,circumscribedCircle = G2Circle.TriangleCircumscribed(positions)});
        //             }
        //
        //             vertexIndex++;
        //         }
        //         
        //         //Remove triangles shared edge with super triangle
        //         _triangles.Clear();
        //         for(var i=kTriangles.Count-1;i>=0;i--)
        //             if(!kTriangles[i].polygon.Any(p=>p<0))  //Is from super triangle
        //                 _triangles.Add(kTriangles[i].polygon);
        //     }
        // }
    }

}

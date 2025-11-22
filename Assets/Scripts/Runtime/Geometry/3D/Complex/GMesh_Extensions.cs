using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Flags]
    public enum EDrawMeshFlag
    {
        Vertices = 1 << 0,
        Triangles = 1 << 1,
        Edges = 1 << 2,
    }
    public partial struct GMesh
    {
         public GMesh(Mesh _mesh):this(_mesh.vertices.Select(p=>(float3)p),_mesh.triangles) { }
         
         public void Populate(Mesh _mesh)
         {
             _mesh.Clear();
             _mesh.SetVertices(vertices.Select(p => (Vector3)p).ToList());
             _mesh.SetTriangles(triangles.Select(p=>(IEnumerable<int>)p).Resolve().ToArray(), 0);
         }

        public float3 GetSupportPoint(float3 _direction)=> vertices.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => GBox.GetBoundingBox(vertices);
        public GSphere GetBoundingSphere() => GSphere.GetBoundingSphere(vertices);
        public void DrawGizmos() => DrawGizmos(EDrawMeshFlag.Vertices | EDrawMeshFlag.Triangles);

        public void DrawGizmos(EDrawMeshFlag _flag)
        {
            if (!Valid)
                return;
            
            if (_flag.IsFlagEnable(EDrawMeshFlag.Vertices))
            {
                foreach (var vertex in vertices)
                    Gizmos.DrawWireSphere(vertex, .01f);
            }

            if (_flag.IsFlagEnable(EDrawMeshFlag.Triangles))
            {
                for (var i = 0; i < triangles.Count; i++)
                    new GTriangle(vertices, triangles[i]).DrawGizmos();
            }

            if (_flag.IsFlagEnable(EDrawMeshFlag.Edges))
            {
                foreach (var edge in edges)
                    new GLine(vertices, edge).DrawGizmos();
            }
        }

        int GetMatchTriangleCount(int _edgeIndex)
        {
            var matchCount = 0;
            var srcEdge = edges[_edgeIndex].Distinct();
            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                var triangle = triangles[i];
                foreach (var edge in triangle.GetEdges())
                {
                    if (edge.Distinct() == srcEdge)
                        matchCount += 1;
                }
            }
            return matchCount;
        }
        
        void CleanUpUnUsedVertices()
        {
            for (var i = vertices.Count - 1; i >= 0; i--)
            {
                var validVertex = false;
                for (var j = 0; j < triangles.Count; j++)
                {
                    if (triangles[j].All(p => p != i))
                        continue;
                    validVertex = true;
                    break;
                }

                if (validVertex) 
                    continue;
                
                for (var j = 0; j < triangles.Count; j++)
                {
                    var triangle = triangles[j];

                    for (var k = 0; k < 3; k++)
                    {
                        var index = triangle[k];
                        if (index > i)
                            index -= 1;
                        triangle[k] = index;
                    }

                    triangles[j] = triangle;
                }
                vertices.RemoveAt(i);
            }
            
        }
        
        public void RemoveEdge(int _edgeIndex)
        {
            var edge = edges[_edgeIndex].Distinct();
            var edgeStart = edge.start;
            var edgeEnd = edge.end;

            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                var triangle = triangles[i];
            
                var matchCount = 0;
                for (var j = 0; j < 3; j++)
                {
                    var index = triangle[j];
                    if (index == edgeStart || index == edgeEnd) 
                        matchCount++;
                
                    if(index == edgeEnd)
                        index = edgeStart;
                    triangle[j] = index;
                }

                if (matchCount == 2)
                {
                    triangles.RemoveAt(i);
                    continue;
                }

                triangles[i] = triangle;
            }
            
            CleanUpUnUsedVertices();
            Ctor();
        }

        public void LoopSubdivision()
        {
            var newVertices = new List<float3>();
            var newTriangles = new List<PTriangle>();
            
            newVertices.AddRange(vertices);
            newTriangles.AddRange(triangles);
            foreach (var pTriangle in triangles)
            {
                var p0 = pTriangle.V0;
                var p1 = pTriangle.V1;
                var p2 = pTriangle.V2;
                var v0 = vertices[pTriangle.V0];
                var v1 = vertices[pTriangle.V1];
                var v2 = vertices[pTriangle.V2];

                var v3 = (v0 + v1) / 2;
                var v4 = (v1 + v2) / 2;
                var v5 = (v2 + v0) / 2;

                var startVertex = newVertices.Count;
                var p3 = startVertex;
                var p4 = startVertex + 1;
                var p5 = startVertex + 2;
                
                newVertices.Add(v3);
                newVertices.Add(v4);
                newVertices.Add(v5);
                
                newTriangles.Add(new PTriangle(p0,p3,p5));
                newTriangles.Add(new PTriangle(p3,p1,p4));
                newTriangles.Add(new PTriangle(p4,p2,p5));
                newTriangles.Add(new PTriangle(p3,p4,p5));
            }

            triangles = newTriangles;
            vertices = newVertices;
        }
    }
}
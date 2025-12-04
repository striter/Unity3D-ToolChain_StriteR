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

            Gizmos.color = Color.white.SetA(.5f);
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
                    new GLine(vertices, edge).Trim(new RangeFloat(.1f,.8f)).DrawGizmos();
            }
        }

        int2 GetAdjacentTriangles(PLine _edge,out int2 _edgeVertices)
        {
            var matchTriangles = -kint2.one;
            _edgeVertices = -kint2.one;
            var srcEdge = _edge.Distinct();
            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                var triangle = triangles[i];
                for (var j = 0; j < 3; j++)
                {
                    var edge = new PLine(triangle[j],triangle[(j + 1) % 3]).Distinct();
                    if (edge != srcEdge) 
                        continue;
                    
                    Debug.Assert(matchTriangles.x == -1 || matchTriangles.y == -1,$"[{nameof(GMesh)}:{nameof(GetAdjacentTriangles)}] Invalid mesh");

                    var edgeVertex = triangle[(j + 2) % 3];
                    if (matchTriangles.x == -1)
                    {
                        matchTriangles.x = i;
                        _edgeVertices.x = edgeVertex;
                    }
                    else if(matchTriangles.y == -1)
                    {
                        matchTriangles.y = i;
                        _edgeVertices.y = edgeVertex;
                    }
                }
            }
            return matchTriangles;
        }

        private static List<PLine> kAdjacentEdgeHelper = new();
        List<PLine> GetAdjacentEdges(int _vertex)
        {
            kAdjacentEdgeHelper.Clear();
            foreach(var edge in edges)
            {
                if(edge.start == _vertex || edge.end == _vertex)
                    kAdjacentEdgeHelper.Add(edge);
            }

            return kAdjacentEdgeHelper;
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

        //https://graphics.stanford.edu/~mdfisher/subdivision.html
        float3 LoopSubdivisionNewVertex(PLine _edge)
        {
            var v0 = vertices[_edge.start];
            var v1 = vertices[_edge.end];
            var matchTriangles = GetAdjacentTriangles(_edge,out var edgeVertices);
            if ((matchTriangles == -1).any()) 
                return (v0 + v1) / 2;

            var v3 = vertices[edgeVertices.x];
            var v4 = vertices[edgeVertices.y];
            return (v0 + v1) * (3/8f) + (v3 + v4) * (1/8f);
        }

        float3 LoopSubdivisionOldVertex(int _vertexIndex)
        {
            var adjacentEdges = GetAdjacentEdges(_vertexIndex);
            var n = adjacentEdges.Count;
            switch (n)
            {
                case 0:
                case 1:
                {
                    Debug.LogError($"[{nameof(GMesh)}|{nameof(LoopSubdivisionOldVertex)}]:Invalid edge count {adjacentEdges.Count} to division");
                    return 0;
                }
                case 2:     //Edge case 
                    return (vertices[adjacentEdges[0].Contract(_vertexIndex)] + vertices[adjacentEdges[1].Contract(_vertexIndex)]) * 1/8f + vertices[_vertexIndex] * 3/4f;
                default:
                {
                    var sum = float3.zero;
                    var beta = n == 3 ? (3 / 16f) : 3/(8f*n);
                    for (var i = 0; i < adjacentEdges.Count; i++)
                    {
                        sum += vertices[adjacentEdges[i].Contract(_vertexIndex)] * beta;
                    }
                    return sum + (1 - n*beta) * vertices[_vertexIndex];
                }
            }
        }
        
        public void LoopSubdivision()
        {
            var newVertices = new List<float3>();
            var newTriangles = new List<PTriangle>();
            
            newVertices.AddRange(vertices);
            newTriangles.AddRange(triangles);
            
            for(var i = triangles.Count - 1 ; i >= 0; i--)
            {
                var pTriangle = triangles[i];
                var p0 = pTriangle.V0;
                var p1 = pTriangle.V1;
                var p2 = pTriangle.V2;
                
                
                var startVertex = newVertices.Count;
                var p3 = startVertex;
                var p4 = startVertex + 1;
                var p5 = startVertex + 2;

                newVertices[pTriangle.V0] = LoopSubdivisionOldVertex(pTriangle.V0);
                newVertices[pTriangle.V1] = LoopSubdivisionOldVertex(pTriangle.V1);
                newVertices[pTriangle.V2] = LoopSubdivisionOldVertex(pTriangle.V2);
                newVertices.Add(LoopSubdivisionNewVertex(new PLine(pTriangle.V0,pTriangle.V1)));
                newVertices.Add(LoopSubdivisionNewVertex(new PLine(pTriangle.V1,pTriangle.V2)));
                newVertices.Add(LoopSubdivisionNewVertex(new PLine(pTriangle.V2,pTriangle.V0)));

                newTriangles.RemoveAt(i);
                newTriangles.Add(new PTriangle(p0,p3,p5));
                newTriangles.Add(new PTriangle(p3,p1,p4));
                newTriangles.Add(new PTriangle(p4,p2,p5));
                newTriangles.Add(new PTriangle(p3,p4,p5));
            }

            triangles = newTriangles;
            vertices = newVertices;
            Ctor();
        }
    }
}
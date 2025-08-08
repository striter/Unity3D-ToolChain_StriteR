using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public struct G2VoronoiDiagram
    {
        private struct VoronoiEdge
        {
            public int vertexIndex;
            public int endVertexIndex;
            public float2 direction;
            public static VoronoiEdge Finite(int _vertexIndex,int _endVertexIndex) => new() {vertexIndex = _vertexIndex,endVertexIndex = _endVertexIndex};
            public static VoronoiEdge Infinite(int _vertexIndex,float2 _direction) => new() {vertexIndex = _vertexIndex,endVertexIndex = -1,direction = _direction};
            public bool IsFinite => endVertexIndex != -1;
            public G2Line ToLine(List<float2> _vertices) => new G2Line(_vertices[vertexIndex],_vertices[endVertexIndex]);
            public G2Ray ToRay(List<float2> _vertices) => new G2Ray(_vertices[vertexIndex],direction);

            public void DrawGizmos(List<float2> _vertices)
            {
                if (IsFinite)
                    ToLine(_vertices).DrawGizmos();
                else
                    ToRay(_vertices).DrawGizmos();
            }
            public static readonly VoronoiEdge kInvalid = new VoronoiEdge {vertexIndex = -1,endVertexIndex = -1};
        }
        
        public G2Graph sites;
        public List<List<int>> siteEdges;
        
        public List<float2> vertices;
        private List<VoronoiEdge> edges;

        private static Dictionary<PLine,List<int>> kEdgeMap = new();
        private static List<PTriangle> kTriangles = new();
        public static G2VoronoiDiagram FromPositions(List<float2> _vertices)
        {
            var sites = _vertices;
            UGeometry.Triangulation(sites,ref kTriangles);
            var siteEdges = new List<List<int>>();
            for(var i = 0 ;i < sites.Count;i++)
                siteEdges.Add(new List<int>());
            var siteGraph = G2Graph.FromTriangles(sites, kTriangles);
            var vertices = kTriangles.Select(pTriangle => new G2Triangle(sites, pTriangle)).Select(triangle => G2Circle.TriangleCircumscribed(triangle.V0, triangle.V1, triangle.V2).center).ToList();
            
            kEdgeMap.Clear();
            foreach (var (triangleIndex,pTriangle) in kTriangles.WithIndex())
            {
                foreach (var edge in pTriangle.GetEdges().Select(p=>p.Distinct()))
                {
                    if (!kEdgeMap.ContainsKey(edge))
                        kEdgeMap.Add(edge, new List<int>());
                    kEdgeMap[edge].Add(triangleIndex);
                }
            }
            
            var edges = new List<VoronoiEdge>();
            foreach (var pair in kEdgeMap)
            {
                var edge = pair.Key;
                var triangleIndexes = pair.Value;
                var voronoiEdge = VoronoiEdge.kInvalid;
                switch (triangleIndexes.Count)
                {
                    default:
                        throw new System.Exception($"Invalid Triangle Count: {triangleIndexes.Count}");
                    case 2:
                        voronoiEdge = VoronoiEdge.Finite(triangleIndexes[0], triangleIndexes[1]);
                        break;
                    case 1:
                    {
                        var edgeTriangleIndex = kTriangles[triangleIndexes[0]];
                        var A = sites[edge.start];
                        var B = sites[edge.end];
                        var C = sites[edgeTriangleIndex.Find(p=>p != edge.start && p != edge.end)];
                        voronoiEdge = VoronoiEdge.Infinite(triangleIndexes[0], umath.cross(A, B, C).normalize());
                        break;
                    }
                }
                var edgeIndex = edges.Count;
                siteEdges[edge.start].Add(edgeIndex);
                siteEdges[edge.end].Add(edgeIndex);
                edges.Add(voronoiEdge);
            }
            
            return new G2VoronoiDiagram { vertices = vertices, edges = edges, sites = siteGraph,siteEdges = siteEdges };
        }


        private static List<VoronoiEdge> kInfiniteEdgeHelper = new();
        public IEnumerable<G2VoronoiCell> ToCells()
        {
            var edges = this.edges;
            var bounds = G2Box.GetBoundingBox(sites.Select(p=>p.position));
            for (var i = 0; i < sites.Count; i++)
            {
                kInfiniteEdgeHelper.Clear();
                var site = sites.nodes[i].position;
                List<float2> cellVertices = new List<float2>();
                foreach(var edgeIndex in siteEdges[i])
                {
                    var edge = edges[edgeIndex];
                    if (edge.IsFinite)
                    {
                        var line = edge.ToLine(vertices);
                        cellVertices.TryAdd(line.start);
                        cellVertices.TryAdd(line.end);
                    }
                    else
                    {
                        kInfiniteEdgeHelper.Add(edge);
                    }
                }

                switch (kInfiniteEdgeHelper.Count)
                {
                    default: throw new System.Exception($"Invalid Infinite Edge Count: {kInfiniteEdgeHelper.Count}");
                    case 0: break;
                    case 2:
                    {
                        var ray1 = kInfiniteEdgeHelper[0].ToRay(vertices);
                        var ray2 = kInfiniteEdgeHelper[1].ToRay(vertices);
                        cellVertices.TryAdd(ray1.origin);
                        cellVertices.TryAdd(ray2.origin);
                        if (ray1.RayIntersection(ray2, out var distance))
                        {
                            cellVertices.Add(ray2.GetPoint(distance));
                        }
                        else
                        {
                            cellVertices.TryAdd(ray1.GetPoint(bounds.extent.magnitude()));
                            cellVertices.TryAdd(ray2.GetPoint(bounds.extent.magnitude()));
                        }
                    }
                        break;
                }
                
                var cell = G2Polygon.ConvexHull(cellVertices);
                foreach (var clipPlane in bounds.GetClipPlanes())
                    cell = cell.Clip(clipPlane);
                yield return new G2VoronoiCell(){site= site,cellEdges = cell};
            }
        }
        
        public void DrawGizmos()
        {
            var cellEdges = vertices;
            edges.Traversal(p=> p.DrawGizmos(cellEdges));
            foreach (var (siteIndex,site) in sites.WithIndex())
            {
                var siteCenter = site.position;
                Gizmos.DrawWireSphere(site.position.to3xz(), .05f);
                foreach (var edgeIndex in siteEdges[siteIndex])
                {
                    var edge = edges[edgeIndex];
                    if (edge.IsFinite)
                    {
                        var line = edge.ToLine(vertices);
                        Gizmos.DrawLine(siteCenter.to3xz(), math.lerp(siteCenter,line.GetPointNormalized(.5f),.9f).to3xz());
                    }
                    else
                    {
                        var ray = edge.ToRay(vertices);
                        Gizmos.DrawLine(siteCenter.to3xz(), math.lerp(siteCenter,ray.GetPoint(1f),.9f).to3xz());
                    }
                }
            }
        }
    }

    public struct G2VoronoiCell
    {
        public float2 site;
        public G2Polygon cellEdges; 
    }
}
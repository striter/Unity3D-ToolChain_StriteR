using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct GMesh : IComplex
    {
        public List<float3> vertices;
        public List<PTriangle> triangles;
        public List<PLine> edges;
        public bool Valid => vertices is { Count: > 0 } && triangles is {Count: > 0} && edges is {Count: >0};
        public float3 Origin { get; private set; }
        public GMesh(IEnumerable<float3> _vertices, IEnumerable<PTriangle> _triangles)
        {
            this = default;
            vertices = _vertices.ToList();
            triangles = _triangles.ToList();
            edges = new();
            Ctor();
        }
        
        public GMesh(IEnumerable<float3> _vertices, IEnumerable<int> _triangles):this(_vertices,_triangles.Chunk(3).Select(p=>new PTriangle(p))) { }
        
        void Ctor()
        {
            edges = triangles.GetDistinctEdges().ToList();
            Origin = vertices.Average();
        }
        
        public static readonly GMesh kDefault = new GMesh(
            new []{new float3(-.5f,-.5f,.5f),new float3(-.5f,.5f,.5f),new float3(.5f,.5f,.5f),new float3(.5f,-.5f,.5f),new float3(-.5f,-.5f,-.5f),new float3(-.5f,.5f,-.5f),new float3(.5f,.5f,-.5f),new float3(.5f,-.5f,-.5f)}, 
            new []{0,1,2,0,2,3,4,5,6,4,6,7,0,1,5,1,2,6,2,3,7,4,5,7,5,6,7,0,3,4});
        
        public static readonly GMesh kEmpty = new GMesh(Array.Empty<float3>(),Array.Empty<int>());
    }
    
}
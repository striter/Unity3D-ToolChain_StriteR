using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GMesh : IComplex
    {
        public List<float3> vertices;
        public List<PTriangle> triangles;
        public List<PLine> edges;
        
        public static readonly GMesh kDefault = new GMesh(
            new []{new float3(-.5f,-.5f,.5f),new float3(-.5f,.5f,.5f),new float3(.5f,.5f,.5f),new float3(.5f,-.5f,.5f),new float3(-.5f,-.5f,-.5f),new float3(-.5f,.5f,-.5f),new float3(.5f,.5f,-.5f),new float3(.5f,-.5f,-.5f)}, 
            new []{0,1,2,0,2,3,4,5,6,4,6,7,0,1,5,1,2,6,2,3,7,4,5,7,5,6,7,0,3,4});
        
        public static readonly GMesh kEmpty = new GMesh(Array.Empty<float3>(),Array.Empty<int>());
        public GMesh(IEnumerable<float3> _vertices, IEnumerable<PTriangle> _triangles)
        {
            this = default;
            vertices = _vertices.ToList();
            triangles = _triangles.ToList();
            edges = triangles.GetDistinctEdges().ToList();
            Ctor();
        }
        
        public GMesh(IEnumerable<float3> _vertices, IEnumerable<int> _triangles):this(_vertices,_triangles.Chunk(3).Select(p=>new PTriangle(p))) { }
        
        public GMesh(Mesh _mesh):this(_mesh.vertices.Select(p=>(float3)p),_mesh.triangles) { }

        void Ctor()
        {
            Origin = vertices.Average();
        }
        
        public float3 Origin { get; set; }
        public float3 GetSupportPoint(float3 _direction)=> vertices.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => GBox.GetBoundingBox(vertices);
        public GSphere GetBoundingSphere() => GSphere.GetBoundingSphere(vertices);
        public void DrawGizmos() => DrawGizmos(EDrawMeshFlag.Vertices | EDrawMeshFlag.Triangles);

        public void DrawGizmos(EDrawMeshFlag _flag)
        {
            if (_flag.IsFlagEnable(EDrawMeshFlag.Vertices))
            {
                foreach (var vertex in vertices)
                    Gizmos.DrawWireSphere(vertex, .01f);
            }

            if (_flag.IsFlagEnable(EDrawMeshFlag.Triangles))
            {
                for (int i = 0; i < triangles.Count; i++)
                    new GTriangle(vertices, triangles[i]).DrawGizmos();
            }

            if (_flag.IsFlagEnable(EDrawMeshFlag.Edges))
            {
                foreach (var edge in edges)
                    new GLine(vertices, edge).DrawGizmos();
            }
        }
    }
    
    [Flags]
    public enum EDrawMeshFlag
    {
        Vertices = 1 << 0,
        Triangles = 1 << 1,
        Edges = 1 << 2,
    }
}
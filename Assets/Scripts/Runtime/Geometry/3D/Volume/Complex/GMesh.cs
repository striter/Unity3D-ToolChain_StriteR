using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public struct GMesh : IVolume
    {
        public float3[] vertices;
        public int[] triangles;
        public GMesh(float3[] _vertices, int[] _triangles)
        {
            vertices = _vertices;
            triangles = _triangles;
            Origin = default;
            Ctor();
        }
        
        public GMesh(IEnumerable<float3> _vertices, IEnumerable<int> _triangles):this(_vertices.ToArray(),_triangles.ToArray()) { }
        void Ctor()
        {
            Origin = vertices.Average();
        }
        
        public float3 Origin { get; set; }
        public float3 GetSupportPoint(float3 _direction)=> vertices.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(vertices);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(vertices);
        public void DrawGizmos() => DrawGizmos(EDrawMeshFlag.Vertices);
        public void DrawGizmos(EDrawMeshFlag _flag)
        {
            if(_flag.IsFlagEnable(EDrawMeshFlag.Vertices))
            {
                foreach (var vertex in vertices)
                    Gizmos.DrawWireSphere(vertex,.01f);
            }
            
            if (_flag.IsFlagEnable(EDrawMeshFlag.Triangles))
            {
                for (int i = 0; i < triangles.Length; i += 3)
                    new GTriangle(vertices[triangles[i]],vertices[triangles[i + 1]],vertices[triangles[i + 2]]).DrawGizmos();
            }
        }
    }
    
    [Flags]
    public enum EDrawMeshFlag
    {
        Vertices,
        Triangles,
        // Edges,
    }
}
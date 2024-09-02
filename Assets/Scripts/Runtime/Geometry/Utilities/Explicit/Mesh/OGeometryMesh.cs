using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Extension.Mesh
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }
    public interface IProceduralMeshGenerator
    {
        public int vertexCount { get; }
        public int triangleCount { get;}
        void Execute(int _index,NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles);
    }
    
}
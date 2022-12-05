using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Explicit.Mesh
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }
    
    public struct Point
    {
        public int index;
        public half2 uv;
        public float3 position;
    }
    
    public struct Axis
    {
        public int index;
        public float3 origin;
        public float3 uDir;
        public float3 vDir;
    }

    public interface IProceduralMeshGenerator
    {
        public int vertexCount { get; }
        public int triangleCount { get;}
        void Execute(int _index,NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles);
    }
    
}
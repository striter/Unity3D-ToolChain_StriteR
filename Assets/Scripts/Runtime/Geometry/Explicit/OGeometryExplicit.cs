using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Geometry.Explicit
{
    public struct Axis
    {
        public int index;
        public float3 origin;
        public float3 uDir;
        public float3 vDir;

        public float3 GetPoint(float2 _uv) => origin + _uv.x * uDir + _uv.y * vDir;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int index;
        public half2 uv;
        public float3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point4
    {
        public float4x3 positions, normals;
    }

}
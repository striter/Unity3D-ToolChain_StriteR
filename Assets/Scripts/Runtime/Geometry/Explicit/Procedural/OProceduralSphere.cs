using System;
using System.ComponentModel;
using Geometry;
using Geometry.Explicit;
using Procedural.Tile;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Explicit.Procedural.Sphere
{
    using static UMath;
    using static KMath;
    using static KProceduralGeometry;

    [Serializable]
    public struct UVSphereGenerator : IProceduralMeshGenerator
    {
        [Clamp(1, 50f)] public float radius;
        [Clamp(1, 500)] public int resolution;
        public static readonly UVSphereGenerator kDefault = new UVSphereGenerator() {radius = .5f, resolution = 20};

        private int resolutionU => 4 * resolution;
        private int resolutionV => 2 * resolution;
        public int vertexCount => (resolutionU + 1) * (resolutionV + 1);
        public int triangleCount => resolutionU * resolutionV * 2;

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            int vertexWidth = resolutionU + 1;

            var vertex = new Vertex();
            vertex.tangent.w = new half(-1);
            for (int j = 0; j <= resolutionV; j++)
            for (int i = 0; i <= resolutionU; i++)
            {
                var curIndex = new Int2(i, j).ToIndex(vertexWidth);
                float2 circle = new float2(i / (float) resolutionU, j / (float) resolutionV);
                float uvRadius = Sin(circle.y * kPI);
                math.sincos(kPI2 * circle.x, out vertex.position.z, out vertex.position.x);
                vertex.position.xz *= uvRadius;
                vertex.position.y = -Cos(kPI * circle.y);
                circle.x = (i - .5f) / resolutionU;
                vertex.texCoord0.xy = (half2) circle;
                math.sincos(kPI2 * circle.x, out var tangentZ, out var tangentX);
                vertex.tangent.xz = new half2(new float2(tangentX, tangentZ));
                vertex.normal = vertex.position.xyz;
                vertex.position *= this.radius;
                _vertices[curIndex] = vertex;
                if (i < resolutionU && j < resolutionV)
                {
                    var iTR = new Int2(i + 1, j + 1).ToIndex(vertexWidth);
                    var iTL = new Int2(i, j + 1).ToIndex(vertexWidth);
                    var iBR = new Int2(i + 1, j).ToIndex(vertexWidth);
                    var iBL = new Int2(i, j).ToIndex(vertexWidth);

                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(iTR, iBR, iBL);
                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(iBL, iTL, iTR);
                }
            }
        }
    }

    [Serializable]
    public struct CubeSphereGenerator : IProceduralMeshGenerator
    {
        [Clamp(1, 50f)] public float radius;
        [Clamp(1, 500)] public int resolution;
        [Rename("Tight As fuck")]public bool tight;
        public static CubeSphereGenerator kDefault = new CubeSphereGenerator() {radius = .5f,resolution = 10,tight = false};

        public int vertexCount => !tight ? kCubeFacingAxisCount * Pow2(resolution + 1) : USphereExplicit.GetCubeSphereVertexCount(resolution);

        public int triangleCount => kCubeFacingAxisCount * resolution * resolution * 2;

        private int GetIndex(int _i, int _j, int _sideIndex)
        {
            if (!tight)
                return _sideIndex * Pow2(resolution + 1) + new Int2(_i, _j).ToIndex(resolution + 1);
            
            return USphereExplicit.GetCubeSphereIndex(_i,_j,resolution,_sideIndex);
        }

        private Point GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            int index = GetIndex(_i, _j, _axis.index);
            float2 uv = new float2(_i / r, _j / r);
            float3 position = UProceduralGeometry.CubeToSphere(_axis.origin + uv.x * _axis.uDir + uv.y * _axis.vDir);
            position *= radius;

            return new Point()
            {
                uv = (half2) uv,
                index = Mathf.Max(index, 0),
                position = position,
            };
        }

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            var vertex = new Vertex();

            for (int k = 0; k < kCubeFacingAxisCount; k++)
            {
                var side = GetCubeFacingAxis(k);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var pTR = GetPoint(i + 1, j + 1, side);
                    var pTL = GetPoint(i, j + 1, side);
                    var pBR = GetPoint(i + 1, j, side);
                    var pBL = GetPoint(i, j, side);

                    vertex.tangent = (half4) new float4(math.normalize(pBR.position - pBL.position), -1f);
                    vertex.normal = math.normalize(math.cross(pTR.position - pBR.position, vertex.tangent.xyz));
                    vertex.position = pTR.position;
                    vertex.texCoord0.xy = pTR.uv;
                    _vertices[pTR.index] = vertex;

                    vertex.normal = math.normalize(math.cross(pTL.position - pBL.position, vertex.tangent.xyz));
                    vertex.position = pTL.position;
                    vertex.texCoord0.xy = pTL.uv;
                    _vertices[pTL.index] = vertex;

                    vertex.normal = math.normalize(math.cross(pTR.position - pBR.position, vertex.tangent.xyz));
                    vertex.position = pBR.position;
                    vertex.texCoord0.xy = pBR.uv;
                    _vertices[pBR.index] = vertex;

                    vertex.normal = math.normalize(math.cross(pTL.position - pBL.position, vertex.tangent.xyz));
                    vertex.position = pBL.position;
                    vertex.texCoord0.xy = pBL.uv;
                    _vertices[pBL.index] = vertex;

                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(pTR.index, pBR.index, pBL.index);
                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(pBL.index, pTL.index, pTR.index);
                }
            }
        }
    }

    [Serializable]
    public struct PolygonSphereGenerator : IProceduralMeshGenerator
    {
        [Clamp(1, 50f)] public float radius;
        [Clamp(1, 500)] public int resolution;
        public bool geodesic;
        [Clamp(1, 20)] public int rhombusCount;
        public bool overlapUV;
        
        public static PolygonSphereGenerator kDefault = new PolygonSphereGenerator() {radius = .5f,resolution = 10 ,geodesic = false,rhombusCount=4};
        public int vertexCount => rhombusCount * (resolution+1) * (resolution+1);
        public int triangleCount => rhombusCount * resolution * resolution * 2;

        private int GetIndex(int _i, int _j, int _sideIndex)
        {
            return _sideIndex * Pow2(resolution + 1) + new Int2(_i, _j).ToIndex(resolution + 1);
        }

        private Point GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            int index = GetIndex(_i, _j, _axis.index);
            float y = (_i + _j) / r-1;
            float2 uv = new float2(_i, _j) / r;

            float3 position = 0;
            if (geodesic)
            {
                math.sincos(kPI*(_j/r),out var sine,out position.y);
                position += _axis.origin + math.lerp(_axis.uDir * sine, _axis.vDir * sine,(float)_i/resolution);
            }
            else
            {
                float2 posUV = (_i + _j)<=resolution?uv:(1f-uv.yx);
                position = new float3(0,y, 0) + _axis.origin + posUV.x * _axis.uDir + posUV.y * _axis.vDir;
            }

            position = math.normalize(position);
            
            if (overlapUV)
                uv = UProceduralGeometry.SphereToUV(position,_axis.index==rhombusCount-1);

            return new Point()
            {
                uv = (half2) uv,
                index = Mathf.Max(index, 0),
                position = position*radius,
            };
        }

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            var vertex = new Vertex();

            for (int k = 0; k < rhombusCount; k++)
            {
                var side = GetOctahedronRhombusAxis(k,rhombusCount);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var pTR = GetPoint(i + 1, j + 1, side);
                    var pTL = GetPoint(i, j + 1, side);
                    var pBR = GetPoint(i + 1, j, side);
                    var pBL = GetPoint(i, j, side);

                    float3 tangentDir = pBR.position - pBL.position;
                    if (math.lengthsq(tangentDir) < float.Epsilon)
                        tangentDir = pTR.position - pBL.position;
                    vertex.tangent = (half4) new float4(math.normalize(tangentDir), -1f);
                    vertex.normal = math.normalize(pTR.position);
                    vertex.position = pTR.position;
                    vertex.texCoord0.xy = pTR.uv;
                    _vertices[pTR.index] = vertex;

                    vertex.normal = math.normalize( pTL.position);
                    vertex.position = pTL.position;
                    vertex.texCoord0.xy = pTL.uv;
                    _vertices[pTL.index] = vertex;

                    vertex.normal = math.normalize( pBR.position);
                    vertex.position = pBR.position;
                    vertex.texCoord0.xy = pBR.uv;
                    _vertices[pBR.index] = vertex;

                    vertex.normal = math.normalize(pBL.position);
                    vertex.position = pBL.position;
                    vertex.texCoord0.xy = pBL.uv;
                    _vertices[pBL.index] = vertex;

                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(pTR.index, pBR.index, pTL.index);
                    _triangles[ti++] = (uint3) new Unity.Mathematics.int3(pTL.index, pBR.index, pBL.index);
                }
            }
        }
    }
}
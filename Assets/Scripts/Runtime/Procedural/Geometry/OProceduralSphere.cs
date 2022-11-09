using System;
using System.ComponentModel;
using Geometry;
using Geometry.Voxel;
using Procedural.Tile;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Procedural.Geometry.Sphere
{
    using static UMath;
    using static KMath;

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
                var curIndex = new TileCoord(i, j).ToIndex(vertexWidth);
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
                    var iTR = new TileCoord(i + 1, j + 1).ToIndex(vertexWidth);
                    var iTL = new TileCoord(i, j + 1).ToIndex(vertexWidth);
                    var iBR = new TileCoord(i + 1, j).ToIndex(vertexWidth);
                    var iBL = new TileCoord(i, j).ToIndex(vertexWidth);

                    _triangles[ti++] = (uint3) new int3(iTR, iBR, iBL);
                    _triangles[ti++] = (uint3) new int3(iBL, iTL, iTR);
                }
            }
        }
    }

    [Serializable]
    public struct CubeSphereGenerator : IProceduralMeshGenerator
    {
        [Clamp(1, 50f)] public float radius;
        [Clamp(1, 500)] public int resolution;
        public static CubeSphereGenerator kDefault = new CubeSphereGenerator() {radius = .5f,resolution = 20};
        public int vertexCount => triangleCount * 4;
        public int triangleCount => KCube.kSideCount * resolution * resolution * 2;

        private int curIndex;
        private GeometryPoint GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            float2 uv = new float2(_i / r, _j / r);
            float3 position = UGeometryVolume.CubeToSphere(_axis.origin + uv.x * _axis.uDir + uv.y * _axis.vDir) ;
            
            return new GeometryPoint()
            {
                uv = (half2) uv,
                position = position* radius,
                index = curIndex++,
            };
        }

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            float r = resolution;
            curIndex = 0;

            var vertex = new Vertex();
            vertex.tangent.w = new half(-1);

            for (int k = 0; k < KCube.kSideCount; k++)
            {
                var side = KCube.GetCubeSide(k);
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

                    _triangles[ti++] = (uint3) new int3(pTR.index, pBR.index, pBL.index);
                    _triangles[ti++] = (uint3) new int3(pBL.index, pTL.index, pTR.index);
                }
            }
        }
    }
    
    [Serializable]
    public struct SeamlessCubeSphereGenerator : IProceduralMeshGenerator
    {
        [Clamp(1, 50f)] public float radius;
        [Clamp(1, 500)] public int resolution;
        [Rename("Tight As fuck")]public bool tight;
        public static SeamlessCubeSphereGenerator kDefault = new SeamlessCubeSphereGenerator() {radius = .5f,resolution = 10,tight = false};

        public int vertexCount =>!tight?
                                KCube.kSideCount*Pow2(resolution+1):
                                (resolution + 1) * (resolution + 1) +
                                 (resolution + 1) * resolution +
                                 resolution * resolution +
                                 resolution * resolution +
                                 (resolution - 1) * (resolution) +
                                 (resolution - 1) * (resolution - 1);

        public int triangleCount => KCube.kSideCount * resolution * resolution * 2;

        private int GetIndex(int _i, int _j, int _sideIndex)
        {
            if (!tight)
                return _sideIndex * Pow2(resolution + 1) + new TileCoord(_i, _j).ToIndex(resolution + 1);
            
            bool firstColumn = _j == 0;
            bool lastColumn = _j == resolution;
            bool firstRow = _i == 0;
            bool lastRow = _i == resolution;
            int index = -1;
            if (_sideIndex == 0)
            {
                index = new TileCoord(_i, _j).ToIndex(resolution + 1);
            }
            else if (_sideIndex == 1)
            {
                if (firstColumn)
                    index = GetIndex(_j, _i, 0);
                else
                    index = (resolution + 1) * (resolution + 1) + new TileCoord(_i, _j - 1).ToIndex(resolution + 1);
            }
            else if (_sideIndex == 2)
            {
                if (firstRow)
                    index = GetIndex(_j, _i, 0);
                else if (firstColumn)
                    index = GetIndex(_j, _i, 1);
                else
                    index = (resolution + 1) * (resolution + 1) + (resolution + 1) * resolution +
                            new TileCoord(_i - 1, _j - 1).ToIndex(resolution);
            }
            else if (_sideIndex == 3)
            {
                if (firstColumn)
                    index = GetIndex(_i, resolution, 1);
                else if (firstRow)
                    index = GetIndex(resolution, _j, 2);
                else
                    index = (resolution + 1) * (resolution + 1) + (resolution + 1) * resolution +
                            resolution * resolution + new TileCoord(_i - 1, _j - 1).ToIndex(resolution);
            }
            else if (_sideIndex == 4)
            {
                if (firstColumn)
                    index = GetIndex(_i, resolution, 2);
                else if (firstRow)
                    index = GetIndex(resolution, _j, 0);
                else if (lastRow)
                    index = GetIndex(_j, resolution, 3);
                else
                    index = (resolution + 1) * (resolution + 1) + (resolution + 1) * resolution +
                            resolution * resolution + (resolution * resolution) +
                            new TileCoord(_i - 1, _j - 1).ToIndex(resolution - 1);
            }
            else if (_sideIndex == 5)
            {
                if (firstColumn)
                    index = GetIndex(_i, resolution, 0);
                else if (lastColumn)
                    index = GetIndex(resolution, _i, 3);
                else if (firstRow)
                    index = GetIndex(resolution, _j, 1);
                else if (lastRow)
                    index = GetIndex(_j, resolution, 4);
                else
                    index = (resolution + 1) * (resolution + 1) + (resolution + 1) * resolution +
                            resolution * resolution + (resolution * resolution) + (resolution - 1) * (resolution) +
                            new TileCoord(_i - 1, _j - 1).ToIndex(resolution - 1);
            }

            return index;
        }

        private GeometryPoint GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            int index = GetIndex(_i, _j, _axis.index);
            float2 uv = new float2(_i / r, _j / r);
            float3 position = UGeometryVolume.CubeToSphere(_axis.origin + uv.x * _axis.uDir + uv.y * _axis.vDir);
            position *= radius;

            return new GeometryPoint()
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

            for (int k = 0; k < KCube.kSideCount; k++)
            {
                var side = KCube.GetCubeSide(k);
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

                    _triangles[ti++] = (uint3) new int3(pTR.index, pBR.index, pBL.index);
                    _triangles[ti++] = (uint3) new int3(pBL.index, pTL.index, pTR.index);
                }
            }
        }
    }
}
using System;
using AOT;
using Procedural.Tile;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Explicit.Mesh.Sphere
{
    using static umath;
    using static kmath;
    using static UCubeExplicit;

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
                float2 uv = new float2(i / (float) resolutionU, j / (float) resolutionV);
                vertex.position = USphereExplicit.UV.GetPoint(uv);
                uv.x = (i - .5f) / resolutionU;
                vertex.texCoord0.xy = (half2) uv;
                math.sincos(kPI2 * uv.x, out var tangentZ, out var tangentX);
                vertex.tangent.xz = new half2(new float2(tangentX, tangentZ));
                vertex.normal = vertex.position.xyz;
                vertex.position *= radius;
                _vertices[new Int2(i, j).ToIndex(vertexWidth)] = vertex;
                if (i < resolutionU && j < resolutionV)
                {
                    var iTR = new Int2(i + 1, j + 1).ToIndex(vertexWidth);
                    var iTL = new Int2(i, j + 1).ToIndex(vertexWidth);
                    var iBR = new Int2(i + 1, j).ToIndex(vertexWidth);
                    var iBL = new Int2(i, j).ToIndex(vertexWidth);

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
        [Rename("Tight As fuck")]public bool tight;
        public static CubeSphereGenerator kDefault = new CubeSphereGenerator() {radius = .5f,resolution = 10,tight = false};

        public int vertexCount => !tight ? kCubeFacingAxisCount * sqr(resolution + 1) : USphereExplicit.Cube.GetVertexCount(resolution);

        public int triangleCount => kCubeFacingAxisCount * resolution * resolution * 2;

        private int GetIndex(int _i, int _j, int _sideIndex)
        {
            if (!tight)
                return _sideIndex * sqr(resolution + 1) + new Int2(_i, _j).ToIndex(resolution + 1);
            
            return USphereExplicit.Cube.GetVertexIndex(_i,_j,resolution,_sideIndex);
        }

        private Point GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            int index = GetIndex(_i, _j, _axis.index);
            float2 uv = new float2(_i / r, _j / r);
            float3 position = USphereExplicit.CubeToSpherePosition(_axis.origin + uv.x * _axis.uDir + uv.y * _axis.vDir);
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
                var side = GetFacingAxis(k);
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
            return _sideIndex * sqr(resolution + 1) + new Int2(_i, _j).ToIndex(resolution + 1);
        }

        private Point GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            float2 uv = new float2(_i, _j) / r;

            float3 position = USphereExplicit.Polygon.GetPoint(uv,_axis,geodesic);
            position = math.normalize(position);
            
            if (overlapUV)
                uv = USphereExplicit.SpherePositionToUV(position,_axis.index==rhombusCount-1);

            return new Point()
            {
                uv = (half2) uv,
                index = Mathf.Max(GetIndex(_i, _j, _axis.index), 0),
                position = position*radius, 
            };
        }

        public delegate void MyFuncDelegate(half4 param1, Point param2, Vertex param3);
        [MonoPInvokeCallback(typeof(MyFuncDelegate))]
        public static Vertex GetVertex(half4 _tangent,Point _p)
        {
            return new Vertex
            {
                tangent = _tangent,
                normal = math.normalize(_p.position),
                position = _p.position,
                texCoord0 = _p.uv
            };
        }

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            var ti = 0;
            for (var k = 0; k < rhombusCount; k++)
            {
                var side = GetOctahedronRhombusAxis(k, rhombusCount);
                for (var j = 0; j < resolution; j++)
                for (var i = 0; i < resolution; i++)
                {
                    var pTR = GetPoint(i + 1, j + 1, side);
                    var pTL = GetPoint(i, j + 1, side);
                    var pBR = GetPoint(i + 1, j, side);
                    var pBL = GetPoint(i, j, side);

                    var tangentDir = pBR.position - pBL.position;
                    if (math.lengthsq(tangentDir) < float.Epsilon)
                        tangentDir = pTR.position - pBL.position;
                    var tangent = (half4)new float4(math.normalize(tangentDir), -1f);
                    
                    _vertices[pTR.index] = GetVertex(tangent, pTR);
                    _vertices[pTL.index] = GetVertex(tangent, pTL);
                    _vertices[pBR.index] = GetVertex(tangent, pBR);
                    _vertices[pBL.index] = GetVertex(tangent, pBL);

                    _triangles[ti++] = (uint3)new int3(pTR.index, pBR.index, pTL.index);
                    _triangles[ti++] = (uint3)new int3(pTL.index, pBR.index, pBL.index);
                }
            }
        }

        public void Execute<T>(NativeArray<T> _vertices, NativeArray<uint3> _triangles,Func<half4,Point,T> _getVertex) where T:struct
        {
            var ti = 0;
            for (var k = 0; k < rhombusCount; k++)
            {
                var side = GetOctahedronRhombusAxis(k,rhombusCount);
                for (var j = 0; j < resolution; j++)
                for (var i = 0; i < resolution; i++)
                {
                    var pTR = GetPoint(i + 1, j + 1, side);
                    var pTL = GetPoint(i, j + 1, side);
                    var pBR = GetPoint(i + 1, j, side);
                    var pBL = GetPoint(i, j, side);

                    var tangentDir = pBR.position - pBL.position;
                    if (math.lengthsq(tangentDir) < float.Epsilon)
                        tangentDir = pTR.position - pBL.position;
                    var tangent = (half4)new float4(math.normalize(tangentDir), -1f);
                    
                    _vertices[pTR.index] = _getVertex(tangent, pTR);
                    _vertices[pTL.index] = _getVertex(tangent, pTL);
                    _vertices[pBR.index] = _getVertex(tangent, pBR);
                    _vertices[pBL.index] = _getVertex(tangent, pBL);

                    _triangles[ti++] = (uint3) new int3(pTR.index, pBR.index, pTL.index);
                    _triangles[ti++] = (uint3) new int3(pTL.index, pBR.index, pBL.index);
                }
            }
        }
    }
}
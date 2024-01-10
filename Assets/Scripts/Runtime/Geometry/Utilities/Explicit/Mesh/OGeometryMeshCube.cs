using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Explicit.Mesh.Cube
{
    [Serializable]
    public struct CubeGenerator : IProceduralMeshGenerator
    {
        [RangeVector(0f,100f)] public Vector3 size;
        [Clamp(1, 500)] public int resolution;
        public static CubeGenerator kDefault = new CubeGenerator() {size = new Vector3(1f,1f,1f),resolution = 20};
        public int vertexCount => triangleCount * 2;
        public int triangleCount => UCubeExplicit.kCubeFacingAxisCount * resolution * resolution * 2;

        private int curIndex;
        private Point GetPoint(int _i, int _j, Axis _axis)
        {
            float r = resolution;
            float2 uv = new float2(_i / r, _j / r);
            return new Point()
            {
                uv = (half2) uv,
                position = (_axis.origin + uv.x * _axis.uDir + uv.y * _axis.vDir)*size/2f,
                index = curIndex++,
            };
        }

        public void Execute(int _index, NativeArray<Vertex> _vertices, NativeArray<uint3> _triangles)
        {
            int ti = 0;
            curIndex = 0;
            var vertex = new Vertex();
            vertex.tangent.w = new half(-1);

            for (int k = 0; k < UCubeExplicit.kCubeFacingAxisCount; k++)
            {
                var side = UCubeExplicit.GetFacingAxis(k);
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
    
}
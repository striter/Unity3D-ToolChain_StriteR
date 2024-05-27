using System;
using System.Runtime.InteropServices;
using Procedural.Tile;
using Runtime;
using Runtime.Geometry.Explicit;
using Runtime.Geometry.Explicit.Mesh.Sphere;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static kmath;
using static umath;

namespace Examples.Rendering.SH
{
    //http://www.ppsloan.org/publications/StupidSH36.pdf
    public class SphericalHarmonics
    {
        public static class Basis
        {
            public const int kAvailableBands = 3;
            public const float kDirectionNormalizationFactor = 16f * kPI / 17f;
            public const float kAmbientNormalizataionFactor = 2 * kSQRTPi;
            
            private static Func<float3, float>[][] kPolynomialBasis = new Func<float3, float>[kAvailableBands+ 1][]{
                new Func<float3,float>[]
                {
                    _ => 1f/(2*kSQRTPi)
                },
                new Func<float3,float>[]
                {
                    p => -kSQRT3*p.y/(2*kSQRTPi),
                    p => kSQRT3*p.z/(2*kSQRTPi),
                    p => -kSQRT3*p.x/(2*kSQRTPi),
                },
                new Func<float3,float>[]{
                    p => kSQRT15 * p.y * p.x/(2*kSQRTPi),
                    p => -kSQRT15 * p.y * p.z /(2*kSQRTPi),
                    p => kSQRT5*(3 * p.z * p.z - 1) / (4*kSQRTPi),
                    p => -kSQRT15 * p.x * p.z /(2*kSQRTPi),
                    p => kSQRT15 * (p.x * p.x - p.y*p.y) / (4*kSQRTPi),
                },
                new Func<float3, float>[] {
                    p => -(kSQRT2 * kSQRT35*p.y*(3*p.x*p.x -p.y*p.y))/(8*kSQRTPi),
                    p => kSQRT105 * p.y*p.x*p.z / (2*kSQRTPi),
                    p =>-(kSQRT2 * kSQRT21 * p.y *(-1 + 5*p.z*p.z)) / (8*kSQRTPi),
                    p => kSQRT7 * p.z *(5*p.z*p.z - 3) / ( 4 * kSQRTPi),
                    p => -(kSQRT2 * kSQRT21 * p.x * (-1 + 5*p.z*p.z)) / (8*kSQRTPi),
                    p => kSQRT105 * (p.x*p.x - p.y*p.y)*p.z / (4*kSQRTPi),
                    p=> -(kSQRT2 * kSQRT35 * p.x * (p.x*p.x - 3*p.y*p.y)) / (8*kSQRTPi),
                },
            };

            public static Func<float3, float> GetBasisFunction(int _band, int _basis)
            {
                if(_band >= kPolynomialBasis[_band].Length)
                    throw new Exception($"Invalid band {_band}");
                
                var functions = kPolynomialBasis[_band];
                var finalBasis = (_basis + functions.Length / 2);
                if(finalBasis >= functions.Length)
                    throw new Exception($"Invalid basis {_basis}");

                return functions[finalBasis];
            }

            public static float GetBasis(int _band, int _basis,float3 _position) => GetBasisFunction(_band,_basis)(_position);
        }
        
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half4 color;
        public half2 texCoord0;
    }
    
    [Serializable]
    public class FSphericalHarmonicsL2VisualizeCore : ARuntimeRendererBase
    {
        protected override string GetInstanceName() => "SHL2";
        [Range(0,SphericalHarmonics.Basis.kAvailableBands)] public int band;
        public int basis;

        public PolygonSphereGenerator m_Generator;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float3 position, normal;
            public half4 tangent;
            public half4 color;
            public half2 texCoord0;
        }
        
        protected override void PopulateMesh(Mesh _mesh, Transform _transform, Transform _viewTransform)
        {
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            var vertexCount = m_Generator.vertexCount;
            var triangleCount = m_Generator.triangleCount;
            var indexCount = triangleCount * 3;
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(5,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0 , VertexAttributeFormat.Float16 , 2);

            meshData.SetVertexBufferParams(vertexCount,vertexAttributes);
            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
            vertexAttributes.Dispose();

            var shBasisFunction = SphericalHarmonics.Basis.GetBasisFunction(band,basis);
            Vertex GetVertex( half4 tangent,Point _p)
            {
                var basisOutput = shBasisFunction(_p.position.normalize());
                return new Vertex()
                {
                    position = _p.position * math.abs(basisOutput),
                    color = (half4)(basisOutput > 0 ? Color.red.to4() : Color.blue.to4()),
                    normal = _p.position.normalize(),
                    tangent = tangent,
                    texCoord0 = _p.uv,
                };
            }
            var vertices = meshData.GetVertexData<Vertex>(0);
            var triangles = meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));
            m_Generator.Execute(vertices,triangles,GetVertex);

            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
            var min = Vector3.zero;
            var max = Vector3.zero;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 compare = vertices[i].position;
                min = Vector3.Min(min,compare);
                max = Vector3.Max(max, compare);
            }

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){vertexCount = vertexCount,bounds = UBoundsIncrement.MinMax(min, max)});

            _mesh.bounds = meshData.GetSubMesh(0).bounds;
            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,_mesh, MeshUpdateFlags.DontRecalculateBounds);
        }
    }
    
    [ExecuteInEditMode]
    public class SHBasisVisualize : ARuntimeRendererMonoBehaviour<FSphericalHarmonicsL2VisualizeCore>
    {
    }
}
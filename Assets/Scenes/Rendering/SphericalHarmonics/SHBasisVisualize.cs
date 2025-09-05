using System;
using System.Runtime.InteropServices;
using Rendering.GI.SphericalHarmonics;
using Runtime;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension.Mesh;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Examples.Rendering.SH
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half4 color;
        public half2 texCoord0;
    }
    
    [ExecuteInEditMode]
    public class SHBasisVisualize : ARendererBase
    {
        [Range(0,SHBasis.kAvailableBands)] public int band;
        public int basis;
        public PolygonSphereGenerator m_Generator = PolygonSphereGenerator.kDefault;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float3 position, normal;
            public half4 tangent;
            public half4 color;
            public half2 texCoord0;
        }
        
        protected override void PopulateMesh(Mesh _mesh, Camera _viewCamera)
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

            var shBasisFunction = SHBasis.GetBasisFunction(band,basis);
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
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){vertexCount = vertexCount,bounds = GBox.Minmax(min, max)});

            _mesh.bounds = meshData.GetSubMesh(0).bounds;
            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,_mesh, MeshUpdateFlags.DontRecalculateBounds);
        }
    }
}
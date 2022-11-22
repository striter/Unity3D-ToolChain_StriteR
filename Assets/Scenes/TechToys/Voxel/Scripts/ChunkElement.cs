using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Geometry;
using TPool;
using TPoolStatic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheVoxel
{
    public enum EChunkElementStatus
    {
        Empty,
        Implicit,
        Explicit,
    }
    
    public class ChunkElement : PoolBehaviour<Int2>
    {
        public EChunkElementStatus m_Status { get; private set; }
        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        private Mesh m_Mesh;
        private Dictionary<Int3, ChunkVoxel> m_Voxels = new Dictionary<Int3, ChunkVoxel>();

        public override void OnPoolSpawn(Int2 _identity)
        {
            base.OnPoolSpawn(_identity);
            transform.position = DVoxel.GetChunkPositionWS(m_PoolID);
            m_Filter = GetComponent<MeshFilter>();
            m_Renderer = GetComponent<MeshRenderer>();
            m_Mesh = new Mesh();
            m_Mesh.MarkDynamic();
            m_Filter.sharedMesh = m_Mesh;
            m_Status = EChunkElementStatus.Empty;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxels.Clear();
        }

        void PopulateImplicit()
        {
            if (m_Status >= EChunkElementStatus.Implicit)
                return;
            m_Status = EChunkElementStatus.Implicit;
            
            float r = DVoxel.kChunkVoxelSize;
            for (int i = 0; i < DVoxel.kChunkVoxelSize; i++)
            for (int j = 0; j < DVoxel.kChunkVoxelSize; j++)
            {
                float2 p2D = new float2(i / r  + m_PoolID.x, j / r + m_PoolID.y) ;
                int heightOffset = (int) (Noise.Perlin.Unit1f2(p2D * 5f) * 5f);
                int groundHeight = DVoxel.kTerrainHeight + heightOffset;
                int dirtOffset = 5 + (int)(Noise.Value.Unit1f2(p2D)*2f);
                int stoneHeight = groundHeight - dirtOffset;
                int dirtHeight = groundHeight;
                
                for (int k = 0; k <= groundHeight; k++)
                {
                    Int3 identity = new Int3(i,k,j);
                    EVoxelType type = EVoxelType.Grass;
                    if (k < dirtHeight)
                        type = EVoxelType.Dirt;
                    if (k < stoneHeight)
                        type = EVoxelType.Stone;

                    float3 p3D = new float3(p2D.x,k/r,p2D.y);
                    if (Noise.Perlin.Unit1f3(p3D * 3) > .5f)
                        continue;
                    
                    m_Voxels.Add(identity,TSPool<ChunkVoxel>.Spawn().Init(identity,type));
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float3 position;
            public float3 normal;
            public half4 tangent;
            public half4 color;
            public half2 uv;
        }
        
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ExplicitJob : IJobFor
        {
            [Readonly] [NativeDisableContainerSafetyRestriction] private NativeArray<MeshVoxelOutput> sides;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<Vertex> vertices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<uint3> indexes;

            public ExplicitJob(NativeArray<MeshVoxelOutput> _sides,NativeArray<Vertex> _vertices,NativeArray<uint3> _indexes)
            {
                sides = _sides;
                vertices = _vertices;
                indexes = _indexes;
            }

            private static readonly float3[] cubeCenters = new float3[]
            {
                new float3(.5f,-.5f,-.5f),new float3(-.5f,-.5f,-.5f),new float3(-.5f,-.5f,.5f),new float3(.5f,-.5f,.5f),
                new float3(.5f,.5f,-.5f),new float3(-.5f,.5f,-.5f),new float3(-.5f,.5f,.5f),new float3(.5f,.5f,.5f)
            };
            public static void GetFacingQuadGeometry(ECubeFacing _facing,out float3 b,out float3 l,out float3 f,out float3 r,out half3 n,out half4 t)
            {
                switch (_facing)
                {
                    default: throw new Exception("Invalid Type");
                    case ECubeFacing.B: { b = cubeCenters[0]; l = cubeCenters[1]; f = cubeCenters[5]; r = cubeCenters[4]; n = (half3)new float3(0f,0f,-1f);t = (half4)new float4(1f,0f,0f,1f);}break;
                    case ECubeFacing.L: { b = cubeCenters[1]; l = cubeCenters[2]; f = cubeCenters[6]; r = cubeCenters[5]; n = (half3)new float3(-1f,0f,0f);t = (half4)new float4(0f,0f,1f,1f);}break;
                    case ECubeFacing.F: { b = cubeCenters[2]; l = cubeCenters[3]; f = cubeCenters[7]; r = cubeCenters[6]; n = (half3)new float3(0f,0f,1f);t = (half4)new float4(-1f,0f,0f,1f); }break;
                    case ECubeFacing.R: { b = cubeCenters[3]; l = cubeCenters[0]; f = cubeCenters[4]; r = cubeCenters[7]; n = (half3)new float3(1f,0f,0f);t = (half4)new float4(0f,0f,-1f,1f); }break;
                    case ECubeFacing.T: { b = cubeCenters[4]; l = cubeCenters[5]; f = cubeCenters[6]; r = cubeCenters[7]; n = (half3)new float3(0f,1f,0f);t = (half4)new float4(1f,0f,0f,1f);}break;
                    case ECubeFacing.D: { b = cubeCenters[3]; l = cubeCenters[2]; f = cubeCenters[1]; r = cubeCenters[0]; n = (half3)new float3(0f,-1f,0f);t = (half4)new float4(-1f,0f,0f,1f); }break;
                }
            }
            
            public void Execute(int _)
            {
                Vertex v = new Vertex();
                int length = sides.Length;

                int vertexIndex = 0;
                int triangleIndex = 0;
                for (int i = 0; i < length; i++)
                {
                    var voxel =  sides[i];
                    float3 centerOS = DVoxel.GetVoxelPositionOS(voxel.identity);
                    for (int j = 0; j < 6; j++)
                    {
                        if (!UByte.PosValid(voxel.sides, j))
                            continue;

                        GetFacingQuadGeometry(UCubeFacing.IndexToFacing(j),out var b,out var l,out var f,out var r,out var n,out var t);
                        v.normal = n;
                        v.tangent = t;

                        v.position = b * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(0, 0);
                        vertices[vertexIndex+0] = v;
                        v.position = l * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(0, 1);
                        vertices[vertexIndex+1] = v;
                        v.position = f * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(1, 1);
                        vertices[vertexIndex+2] = v;
                        v.position = r * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(1, 0);
                        vertices[vertexIndex+3] = v;

                        uint iB = (uint)vertexIndex+0;
                        uint iL = (uint)vertexIndex+1;
                        uint iF = (uint)vertexIndex+2;
                        uint iR = (uint)vertexIndex+3;
                        indexes[triangleIndex] = new uint3(iB, iL, iF);
                        indexes[triangleIndex + 1] = new uint3(iB, iF, iR);
                        vertexIndex += 4;
                        triangleIndex += 2;
                    }
                }
            }
        }

        public bool PopulateExplicit()
        {
            PopulateImplicit();
            if (m_Status >= EChunkElementStatus.Explicit)
                return false;
            m_Status = EChunkElementStatus.Explicit;

            NativeArray<MeshVoxelOutput> outputs = new NativeArray<MeshVoxelOutput>(m_Voxels.Count,Allocator.TempJob,NativeArrayOptions.UninitializedMemory);
            int sideCount = 0;
            int curIndex = 0;
            foreach (var voxel in m_Voxels.Values)
            {
                voxel.Refresh(m_Voxels);
                sideCount += voxel.m_SideCount;
                outputs[curIndex++] = voxel.Output();
            }
            
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            int vertexCount = sideCount * 4;
            int indexCount = sideCount * 6;
            
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(5,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
            vertexAttributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

            meshData.SetVertexBufferParams(vertexCount,vertexAttributes);
            meshData.SetIndexBufferParams(indexCount,IndexFormat.UInt32);
                
            var vertices = meshData.GetVertexData<Vertex>();
            var indexes = meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));
            
            new ExplicitJob(outputs,vertices,indexes).ScheduleParallel(1,1,default).Complete();
            outputs.Dispose();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,sideCount*6){vertexCount = sideCount*4});

            m_Mesh.bounds = UBounds.MinMax(Vector3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkVoxelSize));
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh, MeshUpdateFlags.DontRecalculateBounds);
            vertexAttributes.Dispose();
            return true;
        }

        public bool m_Gizmos;
        private void OnDrawGizmos()
        {
            if (!m_Gizmos)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var voxel in m_Voxels.Values)
            {
                if (voxel.m_Type < 0)
                    continue;
                Gizmos.color = UColor.IndexToColor((int)voxel.m_Type);
                
                Gizmos.DrawWireSphere(DVoxel.GetVoxelPositionOS( voxel.m_Identity),DVoxel.kVoxelSize/4f);
            }
        }
    }
}
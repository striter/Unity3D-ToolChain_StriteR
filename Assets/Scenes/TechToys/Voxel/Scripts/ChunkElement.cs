using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Flags]
    public enum EChunkDirty
    {
        Generation = 1 << 0,
        Relation = 1 << 1,
        Vertices = 1 << 2,
    }

    public class ChunkElement : PoolBehaviour<Int2>
    {
        public EChunkDirty m_Status { get; private set; }
        private MeshFilter m_Filter;
        private Mesh m_Mesh;
        public Dictionary<Int3, ChunkVoxel> m_Voxels { get; private set; } = new Dictionary<Int3, ChunkVoxel>();
        public Dictionary<Int3, byte> m_AOs { get; private set; } = new Dictionary<Int3, byte>();
        public override void OnPoolSpawn(Int2 _identity)
        {
            base.OnPoolSpawn(_identity);
            transform.position = DVoxel.GetChunkPositionWS(m_PoolID);
            m_Filter = GetComponent<MeshFilter>();
            m_Mesh = new Mesh();
            m_Mesh.MarkDynamic();
            m_Filter.sharedMesh = m_Mesh;
            m_Status = EChunkDirty.Generation;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxels.Clear();
        }
        
        public void Tick(float _deltaTime, Dictionary<Int2, ChunkElement> _chunks)
        {
            if (m_Status.IsFlagEnable(EChunkDirty.Generation))
            {
                m_Status &= int.MaxValue - EChunkDirty.Generation;
                PopulateImplicit();
                SetDirty(EChunkDirty.Relation);
                _chunks.GetValueOrDefault(m_PoolID + Int2.kForward)?.SetDirty(EChunkDirty.Relation);
                _chunks.GetValueOrDefault(m_PoolID + Int2.kBack)?.SetDirty(EChunkDirty.Relation);
                _chunks.GetValueOrDefault(m_PoolID + Int2.kLeft)?.SetDirty(EChunkDirty.Relation);
                _chunks.GetValueOrDefault(m_PoolID + Int2.kRight)?.SetDirty(EChunkDirty.Relation);
                return;
            }

            if (m_Status.IsFlagEnable(EChunkDirty.Relation))
            {
                m_Status &= int.MaxValue - EChunkDirty.Relation;
                PopulateRelation(_chunks);
                SetDirty(EChunkDirty.Vertices);
                return;
            }

            if (m_Status.IsFlagEnable(EChunkDirty.Vertices))
            {
                m_Status &= int.MaxValue - EChunkDirty.Vertices;
                PopulateVertices();
                return;
            }
        }

        
        public void SetDirty(EChunkDirty _dirty)
        {
            m_Status |= _dirty;
        }

        void PopulateImplicit()
        {
            NativeList<VoxelInput> list = new NativeList<VoxelInput>(Allocator.TempJob);
            new ImplicitJob(m_PoolID,list).ScheduleParallel(1,1,default).Complete();
            foreach (var input in list)
                m_Voxels.Add(input.identity, new ChunkVoxel(){identity = input.identity,type = input.type,sideCount = 0,sideGeometry = byte.MinValue});
            list.Dispose();
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct VoxelInput
        {
            public Int3 identity;
            public EVoxelType type;
        }
        
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ImplicitJob : IJobFor
        {
            private Int2 idenitity;
            [Readonly] [NativeDisableContainerSafetyRestriction]  private NativeList<VoxelInput> voxelInputs;
            public ImplicitJob(Int2 _identity,NativeList<VoxelInput> _voxelInputs )
            {
                idenitity = _identity;
                voxelInputs = _voxelInputs;
            }
            
            public void Execute(int _)
            {
                float r = DVoxel.kChunkSize;
                for (int i = 0; i < DVoxel.kChunkSize; i++)
                for (int j = 0; j < DVoxel.kChunkSize; j++)
                {
                    float2 p2D = new float2(i / r  + idenitity.x, j / r + idenitity.y) ;
                    int heightOffset = (int) (Noise.Perlin.Unit1f2(p2D * 5f) * 5f);
                    int groundHeight = DVoxel.kTerrainHeight + heightOffset;
                    int dirtOffset = 5 + (int)(Noise.Value.Unit1f2(p2D)*2f);
                    int stoneHeight = groundHeight - dirtOffset;
                    int dirtHeight = groundHeight;
                
                    for (int k = 0; k <= groundHeight; k++)
                    {
                        EVoxelType type = EVoxelType.Air;
                        if (k < stoneHeight)
                            type = EVoxelType.Stone;
                        else if (k < dirtHeight)
                            type = EVoxelType.Dirt;
                        else if (k == groundHeight)
                            type = EVoxelType.Grass;

                        float3 p3D = new float3(p2D.x,k/r,p2D.y);
                        if (Noise.Perlin.Unit1f3(p3D * 3) > .5f)
                            continue;
                        
                        voxelInputs.Add(new VoxelInput(){identity =  new Int3(i,k,j),type = type});
                    }
                }
            }
        }

        void PopulateRelation(Dictionary<Int2,ChunkElement> _chunks)
        {
            Func<Int3, ChunkVoxel> getVoxel = (_voxelID) =>
            {
                Int2 chunkOffset = Int2.kZero;

                if (_voxelID.x < 0)
                    chunkOffset.x -= 1;
                if (_voxelID.x >= DVoxel.kChunkSize)
                    chunkOffset.x += 1;
                if (_voxelID.z < 0)
                    chunkOffset.y -= 1;
                if (_voxelID.z >= DVoxel.kChunkSize)
                    chunkOffset.y += 1;

                var chunk = m_PoolID + chunkOffset;
                if (!_chunks.ContainsKey(chunk))
                    return ChunkVoxel.kInvalid;

                var voxels = _chunks[chunk].m_Voxels;
                _voxelID = (_voxelID + DVoxel.kChunkSize) % DVoxel.kChunkSize;
                return voxels.ContainsKey(_voxelID) ? voxels[_voxelID] : ChunkVoxel.kInvalid;
            };


            TSPoolList<ChunkVoxel>.Spawn(out var voxelsDelta);

            foreach (var voxelID in m_Voxels.Keys)
            {
                ChunkVoxel srcVoxel = m_Voxels[voxelID];
                ChunkVoxel dstVoxel = srcVoxel;
                for (int i = 0; i < 6; i++)
                {
                    var facingVoxel = getVoxel(voxelID + UCube.GetCubeOffset(UCube.IndexToFacing(i)));
                    bool sideValid =  facingVoxel.type != EVoxelType.Air;
                    var sideByte = (sideValid ? 0 : 1) << i;
                    dstVoxel.sideGeometry += (byte)sideByte;
                    dstVoxel.sideCount += sideValid?0:1;
                }

                for (int i = 0; i < 8; i++)
                {
                    UCube.GetCubeAORelation(UCube.IndexToCorner(i), out var side1, out var side2, out var corner);
                    var side1Voxel = getVoxel(voxelID + side1);
                    var side2Voxel = getVoxel(voxelID + side2);
                    var cornerVoxel = getVoxel(voxelID + corner);
                    dstVoxel.cornerAO += side1Voxel.type != EVoxelType.Air? 1 << (3 * i ) : 0;
                    dstVoxel.cornerAO += side2Voxel.type != EVoxelType.Air? 1 << (3 * i + 1) : 0;
                    dstVoxel.cornerAO += cornerVoxel.type != EVoxelType.Air? 1 << (3 * i + 2) : 0;
                }
            
                if(dstVoxel.Equals(srcVoxel))
                    continue;
                
                voxelsDelta.Add(dstVoxel);
            }

            foreach (var voxel in voxelsDelta)
                m_Voxels[voxel.identity] = voxel;
            
            TSPoolList<ChunkVoxel>.Recycle(voxelsDelta);
        }
        
        public void PopulateVertices()
        {
            NativeArray<ChunkVoxel> outputs = new NativeArray<ChunkVoxel>(m_Voxels.Count,Allocator.TempJob,NativeArrayOptions.UninitializedMemory);
            int sideCount = 0;
            int curIndex = 0;
            
            foreach (var voxel in m_Voxels.Values)
            {
                sideCount += voxel.sideCount;
                outputs[curIndex++] = voxel;
            }
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            int vertexCount = sideCount * 4;
            int indexCount = sideCount * 6;
            
            new ExplicitJob(meshData, outputs,vertexCount,indexCount).ScheduleParallel(1,1,default).Complete();
            outputs.Dispose();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,sideCount*6){vertexCount = sideCount*4});

            m_Mesh.bounds = UBounds.MinMax(Vector3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkSize));
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh, MeshUpdateFlags.DontRecalculateBounds);
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ExplicitJob : IJobFor
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Vertex
            {
                public float3 position;
                public float3 normal;
                public half4 tangent;
                public half4 color;
                public half2 uv;
            }
            
            [Readonly] [NativeDisableContainerSafetyRestriction] private NativeArray<ChunkVoxel> voxels;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<Vertex> vertices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<uint3> indexes;

            public ExplicitJob(Mesh.MeshData _meshData, NativeArray<ChunkVoxel> _voxels,int _vertexCount,int _indexCount)
            {
                voxels = _voxels;
                
                var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(5,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
                vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position , VertexAttributeFormat.Float32,3);
                vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal , VertexAttributeFormat.Float32 , 3);
                vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent , VertexAttributeFormat.Float16 , 4);
                vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.Color , VertexAttributeFormat.Float16 , 4);
                vertexAttributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

                _meshData.SetVertexBufferParams(_vertexCount,vertexAttributes);
                _meshData.SetIndexBufferParams(_indexCount,IndexFormat.UInt32);
                
                vertices = _meshData.GetVertexData<Vertex>();
                indexes = _meshData.GetIndexData<uint>().Reinterpret<uint3>(sizeof(uint));

                vertexAttributes.Dispose();
            }

            public static void GetFacingQuadGeometry(ECubeFacing _facing,out float3 b,out float3 l,out float3 f,out float3 r,out half3 n,out half4 t,out int cb,out int cl,out int cf,out int cr)
            {
                switch (_facing)
                {
                    default: throw new InvalidEnumArgumentException();
                    case ECubeFacing.B: { cb = 0; cl = 1; cf = 5; cr = 4; n = (half3)new float3(0f,0f,-1f);t = (half4)new float4(1f,0f,0f,1f);}break;
                    case ECubeFacing.L: { cb = 1; cl = 2; cf = 6; cr = 5; n = (half3)new float3(-1f,0f,0f);t = (half4)new float4(0f,0f,1f,1f);}break;
                    case ECubeFacing.F: { cb = 2; cl = 3; cf = 7; cr = 6; n = (half3)new float3(0f,0f,1f);t = (half4)new float4(-1f,0f,0f,1f); }break;
                    case ECubeFacing.R: { cb = 3; cl = 0; cf = 4; cr = 7; n = (half3)new float3(1f,0f,0f);t = (half4)new float4(0f,0f,-1f,1f); }break;
                    case ECubeFacing.T: { cb = 4; cl = 5; cf = 6; cr = 7; n = (half3)new float3(0f,1f,0f);t = (half4)new float4(1f,0f,0f,1f);}break;
                    case ECubeFacing.D: { cb = 0; cl = 3; cf = 2; cr = 1; n = (half3)new float3(0f,-1f,0f);t = (half4)new float4(-1f,0f,0f,1f); }break;
                }
                b = KCube.kVoxelPositions[cb]; l = KCube.kVoxelPositions[cl];  f = KCube.kVoxelPositions[cf]; r = KCube.kVoxelPositions[cr];
            }

            static float ConvertToAO(int _aoSrc,int _corner)
            {
                var aoParameters = _aoSrc >> (_corner*3);
                var side1 = (aoParameters & 1);
                var side2 = (aoParameters & 2) >> 1;
                if (side1 + side2 == 2)
                    return 0;
                
                var corner = (aoParameters & 4) >> 2;
                return (3-side1-side2-corner)/3f;
            }
            
            public void Execute(int _)
            {
                Vertex v = new Vertex();
                int length = voxels.Length;

                int vertexIndex = 0;
                int triangleIndex = 0;
                for (int i = 0; i < length; i++)
                {
                    var voxel =  voxels[i];
                    if (voxel.sideCount == 0)
                        continue;

                    v.color = (half4)DVoxel.GetVoxelBaseColor(voxel.type);
                    float3 centerOS = DVoxel.GetVoxelPositionOS(voxel.identity);
                    for (int j = 0; j < 6; j++)
                    {
                        if (!UByte.PosValid(voxel.sideGeometry, j))
                            continue;

                        var facing = UCube.IndexToFacing(j);
                        GetFacingQuadGeometry(facing,
                            out var b,out var l,out var f,out var r,out var n,out var t,
                            out var cb,out var cl,out var cf,out var cr);

                        float aob = ConvertToAO(voxel.cornerAO, cb),
                            aol = ConvertToAO(voxel.cornerAO, cl),
                            aof = ConvertToAO(voxel.cornerAO, cf),
                            aor = ConvertToAO(voxel.cornerAO, cr);
                        
                        v.normal = n;
                        v.tangent = t;

                        v.position = b * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(0, 0);
                        v.color.w = (half)aob;
                        vertices[vertexIndex+0] = v;
                        v.position = l * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aol;
                        v.uv = (half2) new float2(0, 1);
                        vertices[vertexIndex+1] = v;
                        v.position = f * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aof;
                        v.uv = (half2) new float2(1, 1);
                        vertices[vertexIndex+2] = v;
                        v.position = r * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half) aor;
                        v.uv = (half2) new float2(1, 0);
                        vertices[vertexIndex+3] = v;

                        uint iB = (uint)vertexIndex+0;
                        uint iL = (uint)vertexIndex+1;
                        uint iF = (uint)vertexIndex+2;
                        uint iR = (uint)vertexIndex+3;
                        if (aol + aor > aof + aob)
                        {
                            indexes[triangleIndex] = new uint3(iL, iR, iB);
                            indexes[triangleIndex + 1] = new uint3(iL, iF, iR);
                        }
                        else
                        {
                            indexes[triangleIndex] = new uint3(iB, iL, iF);
                            indexes[triangleIndex + 1] = new uint3(iB, iF, iR);
                        }
                        
                        vertexIndex += 4;
                        triangleIndex += 2;
                    }
                }
            }
        }

    }
}
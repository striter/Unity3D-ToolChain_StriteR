using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Geometry;
using TPool;
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

        [NativeDisableContainerSafetyRestriction] public NativeHashMap<Int3, ChunkVoxel> m_Voxels;
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

        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            if(m_Voxels.IsCreated)
                m_Voxels.Dispose();
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
            m_Voxels =  new NativeHashMap<Int3, ChunkVoxel>(list.Length,Allocator.Persistent);
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

            private static readonly int kHalfMax = 10000;
            public void Execute(int _)
            {
                float r = DVoxel.kChunkSize;
                for (int i = 0; i < DVoxel.kChunkSize; i++)
                for (int j = 0; j < DVoxel.kChunkSize; j++)
                {
                    float2 p2D = new float2(i / r  + idenitity.x + kHalfMax, j / r + idenitity.y + kHalfMax) ;

                    ETerrainForm form = ETerrainForm.Plane;
                    float terrainFormPerlin = Noise.Perlin.Unit1f2(p2D * .5f);
                    if (terrainFormPerlin > 0)
                        form = ETerrainForm.Mountains;
                    
                    EVoxelType surfaceType = EVoxelType.Grass;

                    float terrainDensity = 3f;
                    RangeInt stoneRandom = new RangeInt(DVoxel.kTerrainHeight,1);
                    RangeInt dirtRandom = new RangeInt(4,1);
                    if (form == ETerrainForm.Mountains)
                    {
                        terrainDensity = 5f;
                        surfaceType = EVoxelType.Snow;
                        dirtRandom = new RangeInt(0, 3);
                        stoneRandom = new RangeInt(DVoxel.kTerrainHeight, DVoxel.kMountainHeight);
                    }

                    float terrainRandom = Noise.Perlin.Unit1f2(p2D * terrainDensity);
                    int stoneHeight = stoneRandom.start + (int) (terrainRandom* stoneRandom.length);
                    int dirtHeight = dirtRandom.start + (int) (terrainRandom*dirtRandom.length);

                    int stoneStart = stoneHeight;
                    int dirtStart = stoneStart + dirtHeight;
                    int surface = dirtStart;
                    
                    for (int k = 0; k <= surface; k++)
                    {
                        EVoxelType voxel = EVoxelType.Air;
                        if(k < stoneStart)
                            voxel = EVoxelType.Stone;
                        else if (k < dirtStart)
                            voxel = EVoxelType.Dirt;
                        else if (k == surface)
                            voxel = surfaceType;

                        //Caves
                        float3 p3D = new float3(p2D.x,k/r,p2D.y);
                        if (Noise.Perlin.Unit1f3(p3D * 3) > .5f)
                            continue;
                        
                        voxelInputs.Add(new VoxelInput(){identity =  new Int3(i,k,j),type = voxel});
                    }
                }
            }
        }

        void PopulateRelation(Dictionary<Int2,ChunkElement> _chunks)
        {
            NativeList<ChunkVoxel> voxelsDelta = new NativeList<ChunkVoxel>(Allocator.TempJob);
            var back = _chunks.TryGetValue(m_PoolID + Int2.kBack, out var backChunk)?backChunk.m_Voxels : new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.TempJob);
            var left = _chunks.TryGetValue(m_PoolID + Int2.kLeft, out var leftChunk)?leftChunk.m_Voxels : new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.TempJob);
            var forward = _chunks.TryGetValue(m_PoolID + Int2.kForward, out var forwardChunk)?forwardChunk.m_Voxels : new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.TempJob);
            var right = _chunks.TryGetValue(m_PoolID + Int2.kRight, out var rightChunk)?rightChunk.m_Voxels : new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.TempJob);

            new RefreshRelationJob(voxelsDelta, m_Voxels, back,left,forward,right).ScheduleParallel(1,1,default).Complete();

            foreach (var voxel in voxelsDelta)
            {
                m_Voxels.Remove(voxel.identity);
                m_Voxels.Add(voxel.identity,voxel);
            }

            if (back.IsEmpty) back.Dispose();
            if (left.IsEmpty) left.Dispose();
            if (forward.IsEmpty) forward.Dispose();
            if (right.IsEmpty) right.Dispose();
            voxelsDelta.Dispose();
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct RefreshRelationJob : IJobFor
        {
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> voxelsDelta;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3, ChunkVoxel> curVoxels;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private readonly NativeHashMap<Int3, ChunkVoxel> backVoxels;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private readonly NativeHashMap<Int3, ChunkVoxel> leftVoxels;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private readonly NativeHashMap<Int3, ChunkVoxel> forwardVoxels;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private readonly NativeHashMap<Int3, ChunkVoxel> rightvoxels;
            public RefreshRelationJob(NativeList<ChunkVoxel> _voxelsDelta,NativeHashMap<Int3, ChunkVoxel> _curVoxels, NativeHashMap<Int3, ChunkVoxel> _backVoxels,NativeHashMap<Int3, ChunkVoxel> _leftVoxels,NativeHashMap<Int3, ChunkVoxel> _forwardVoxels,NativeHashMap<Int3, ChunkVoxel> _rightVoxels)
            {
                voxelsDelta = _voxelsDelta;
                curVoxels = _curVoxels;
                backVoxels = _backVoxels;
                leftVoxels = _leftVoxels;
                forwardVoxels = _forwardVoxels;
                rightvoxels = _rightVoxels;
            }
            
            ChunkVoxel GetVoxel(Int3 _voxelID)
            {
                ChunkVoxel voxel = ChunkVoxel.kInvalid;
                NativeHashMap<Int3, ChunkVoxel> voxels = curVoxels;
                if (_voxelID.x < 0)
                    voxels = leftVoxels;
                if (_voxelID.x >= DVoxel.kChunkSize)
                    voxels = rightvoxels;
                if (_voxelID.z < 0)
                    voxels = backVoxels;
                if (_voxelID.z >= DVoxel.kChunkSize)
                    voxels = forwardVoxels;

                if(!voxels.TryGetValue((_voxelID + DVoxel.kChunkSize) % DVoxel.kChunkSize,out voxel))
                    voxel = ChunkVoxel.kInvalid;
                return voxel;
            }
            
            public void Execute(int _)
            {
                var keys = curVoxels.GetKeyArray(Allocator.Temp);
                var length = keys.Length;
                for(int index=0;index<length;index++)
                {
                    var voxelID = keys[index];
                    ChunkVoxel srcVoxel = curVoxels[voxelID];
                    ChunkVoxel dstVoxel = new ChunkVoxel() {identity = srcVoxel.identity, type = srcVoxel.type};
                    for (int i = 0; i < 6; i++)
                    {
                        var facingVoxel = GetVoxel(voxelID + UCube.GetCubeOffset(UCube.IndexToFacing(i)));
                        bool sideValid =  facingVoxel.type != EVoxelType.Air;
                        dstVoxel.sideGeometry += (byte)((sideValid ? 1 : 0) << i);
                        dstVoxel.sideCount += (byte)(sideValid?0:1);
                    }

                    if(dstVoxel.sideCount==0)
                        continue;
                    for (int i = 0; i < 8; i++)
                    {
                        var cornerVoxel =  GetVoxel(voxelID+KCube.kCornerIdentity[i]);
                        bool cornerValid = cornerVoxel.type != EVoxelType.Air;
                        var cornerByte = (cornerValid ? 1 : 0) << i;
                        dstVoxel.cornerGeometry += (byte) cornerByte;
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        var intervalVoxel =  GetVoxel(voxelID + KCube.kIntervalIdentity[i]);
                        bool interValid = intervalVoxel.type != EVoxelType.Air;
                        var intervalByte = (interValid ? 1 : 0) << i;
                        dstVoxel.intervalGeometry += (ushort)intervalByte;
                    }
                
                    if(dstVoxel.Equals(srcVoxel))
                        continue;
                
                    voxelsDelta.Add(dstVoxel);
                }
                keys.Dispose();
            }
        }
        
        public void PopulateVertices()
        {
            NativeArray<ChunkVoxel> outputs = m_Voxels.GetValueArray(Allocator.TempJob);
            int sideCount = 0;
            foreach (var voxel in outputs)
                sideCount += voxel.sideCount;
            
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
            
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<ChunkVoxel> voxels;
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

            static float ConvertToAO(int _intervalSrc,int _side1,int _side2,int _cornerByte,int _corner)
            {
                var side1 = (_intervalSrc >> _side1) & 1;
                var side2 = (_intervalSrc >> _side2) & 1;

                var corner = (_cornerByte >> _corner) & 1;
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
                        if (UByte.PosValid(voxel.sideGeometry, j))
                            continue;

                        var facing = UCube.IndexToFacing(j);
                        UCube.GetCornerGeometry(facing ,out var b,out var l,out var f,out var r,out var n,out var t);
                        float aob = ConvertToAO(voxel.intervalGeometry, b.side1, b.side2, voxel.cornerGeometry,b.corner),
                            aol = ConvertToAO(voxel.intervalGeometry, l.side1, l.side2, voxel.cornerGeometry, l.corner),
                            aof = ConvertToAO(voxel.intervalGeometry, f.side1, f.side2, voxel.cornerGeometry, f.corner),
                            aor = ConvertToAO(voxel.intervalGeometry, r.side1, r.side2, voxel.cornerGeometry, r.corner);
                        
                        v.normal = n;
                        v.tangent = t;

                        v.position = b.position * DVoxel.kVoxelSize + centerOS;
                        v.uv = (half2) new float2(0, 0);
                        v.color.w = (half)aob;
                        vertices[vertexIndex+0] = v;
                        v.position = l.position * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aol;
                        v.uv = (half2) new float2(0, 1);
                        vertices[vertexIndex+1] = v;
                        v.position = f.position * DVoxel.kVoxelSize + centerOS;
                        v.color.w = (half)aof;
                        v.uv = (half2) new float2(1, 1);
                        vertices[vertexIndex+2] = v;
                        v.position = r.position * DVoxel.kVoxelSize + centerOS;
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
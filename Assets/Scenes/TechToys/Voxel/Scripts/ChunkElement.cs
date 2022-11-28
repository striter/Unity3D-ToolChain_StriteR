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

        private NativeHashMap<Int3, int> m_Indexes;
        private NativeList<Int3> m_Keys;
        private NativeList<ChunkVoxel> m_Values;
        private int m_SideCount;

        private ImplicitJob m_ImplicitJob;
        private RefreshRelationJob m_RefreshRelationJob;
        private ExplicitJob m_ExplicitJob;
        
        public override void OnPoolCreate(Action<Int2> _doRecycle)
        {
            base.OnPoolCreate(_doRecycle);
            m_Filter = GetComponent<MeshFilter>();
            m_Mesh = new Mesh(){name = "Chunk Mesh",hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            
            m_Filter.sharedMesh = m_Mesh;
            m_Indexes = new NativeHashMap<Int3, int>(0,Allocator.Persistent);
            m_Keys = new NativeList<Int3>(Allocator.Persistent);
            m_Values = new NativeList<ChunkVoxel>(Allocator.Persistent);
        }

        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            m_Indexes.Dispose();
            m_Keys.Dispose();
            m_Values.Dispose();
        }
        
        public override void OnPoolSpawn(Int2 _identity)
        {
            base.OnPoolSpawn(_identity);
            transform.position = DVoxel.GetChunkPositionWS(m_PoolID);
            m_Status = EChunkDirty.Generation;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            Clear();
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

        void SetDirty(EChunkDirty _dirty)
        {
            m_Status |= _dirty;
        }

        void Clear()
        {
            m_Mesh.Clear();
            m_Indexes.Clear();
            m_Keys.Clear();
            m_Values.Clear();
        }
        
        void PopulateImplicit()
        {
            new ImplicitJob(m_PoolID,m_Indexes,m_Keys,m_Values).ScheduleParallel(1,1,default).Complete();
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ImplicitJob : IJobFor
        {
            private Int2 identity;
            private int startIndex;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3, int> indexes;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<Int3> keys;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> values;
            public ImplicitJob(Int2 _identity,NativeHashMap<Int3, int> _indexes,NativeList<Int3> _keys,NativeList<ChunkVoxel> _values)
            {
                startIndex = 0;
                identity = _identity;
                indexes = _indexes;
                keys = _keys;
                values = _values;
            }

            void Insert(Int3 _identity,EVoxelType _type)
            {
                indexes.Add(_identity,startIndex++);
                keys.Add(_identity);
                values.Add(new ChunkVoxel(){identity = _identity ,type = _type});
            }

            bool CaveValidation(float2 _p2D,int _height)
            {
                float r = DVoxel.kChunkSize;
                float3 p3D = new float3(_p2D.x,_height/r,_p2D.y);
                return Noise.Perlin.Unit1f3(p3D * 3) > .55f;
            }
            
            private static readonly int kHalfMax = 10000;
            public void Execute(int _)
            {
                float r = DVoxel.kChunkSize;
                for (int i = 0; i < DVoxel.kChunkSize; i++)
                for (int j = 0; j < DVoxel.kChunkSize; j++)
                {
                    float2 p2D = new float2(i / r  + identity.x + kHalfMax, j / r + identity.y + kHalfMax) ;

                    ETerrainForm terrainForm = ETerrainForm.Plane;
                    float terrainFormRange = Noise.Perlin.Unit1f2(p2D * .5f);
                    if (terrainFormRange > 0)
                        terrainForm = ETerrainForm.Mountains;

                    int surfaceHeight = DVoxel.kTerrainHeight;
                    switch (terrainForm)
                    {
                        case ETerrainForm.Mountains:
                        {
                            // int terrainHeight = 
                            //
                            // surfaceType = EVoxelType.Snow;
                            // terrainHeight += new RangeInt(0, DVoxel.kMountainHeight).GetValueContains(terrainFormRange);
                        }
                            break;
                        case ETerrainForm.Plane:
                        {
                            float terrainRandom = Noise.Perlin.Unit1f2(p2D * 3f) * .5f +.5f;
                            float noiseRandom = Noise.Value.Unit1f2(p2D) *.5f + .5f;

                            RangeInt terrainRange = new RangeInt(-2,5);
                            RangeInt dirtRange = new RangeInt(3, 2);
                            surfaceHeight += terrainRange.GetValueContains(terrainRandom);
                            int stoneHeight = surfaceHeight;
                            surfaceHeight += dirtRange.GetValueContains(noiseRandom);
                            int dirtHeight = surfaceHeight;
                            surfaceHeight += 1;

                            for (int k = 1; k <= surfaceHeight; k++)
                            {
                                EVoxelType type = EVoxelType.Air;
                                if(k <= stoneHeight)
                                    type = EVoxelType.Stone;
                                else if (k <= dirtHeight)
                                    type = EVoxelType.Dirt;
                                else if (k == surfaceHeight)
                                    type = EVoxelType.Grass;
                            
                                if (CaveValidation(p2D,k))
                                    continue;
                                Insert(new Int3(i,k,j),type);
                            }
                        }
                        break;
                    }
                    Insert(new Int3(i, 0, j), EVoxelType.BedRock);
                }
            }
        }

        void PopulateRelation(Dictionary<Int2,ChunkElement> _chunks)
        {
            var back = _chunks.TryGetValue(m_PoolID + Int2.kBack, out var backChunk);
            var left = _chunks.TryGetValue(m_PoolID + Int2.kLeft, out var leftChunk);
            var forward = _chunks.TryGetValue(m_PoolID + Int2.kForward, out var forwardChunk);
            var right = _chunks.TryGetValue(m_PoolID + Int2.kRight, out var rightChunk);
            
            var emptyIndexes = new NativeHashMap<Int3, int>(0,Allocator.Persistent);
            var emptyValues = new NativeList<ChunkVoxel>(0, Allocator.Persistent);
        
            NativeArray<int> sideCount = new NativeArray<int>(1,Allocator.TempJob);
            var refreshJob = new RefreshRelationJob(sideCount,m_Indexes,m_Keys,m_Values,
                back?backChunk.m_Indexes : emptyIndexes,back?backChunk.m_Values : emptyValues,
                left?leftChunk.m_Indexes : emptyIndexes,left?leftChunk.m_Values : emptyValues,
                forward?forwardChunk.m_Indexes:emptyIndexes, forward?forwardChunk.m_Values : emptyValues,
                right?rightChunk.m_Indexes : emptyIndexes,right?rightChunk.m_Values : emptyValues);
            refreshJob.ScheduleParallel(1,1,default).Complete();
            m_SideCount = sideCount[0];
            sideCount.Dispose();
            
            emptyIndexes.Dispose();
            emptyValues.Dispose();
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct RefreshRelationJob : IJobFor
        {
            private NativeArray<int> sideCount;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<Int3> keys;
            [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> values;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeHashMap<Int3, int> indexes,backIndexes,leftIndexes,forwardIndexes,rightIndexes;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<ChunkVoxel> backValues,leftValues,forwardValues,rightValues;
            
            public RefreshRelationJob(NativeArray<int> _sideCount, NativeHashMap<Int3,int> _indexes,NativeList<Int3> _keys,NativeList<ChunkVoxel> _values, 
                NativeHashMap<Int3,int> _backIndexes,NativeList<ChunkVoxel> _backVoxels,
                NativeHashMap<Int3,int> _leftIndexes,NativeList<ChunkVoxel> _leftVoxels,
                NativeHashMap<Int3,int> _forwardIndexes,NativeList<ChunkVoxel> _forwardVoxels,
                NativeHashMap<Int3,int> _rightIndexes,NativeList<ChunkVoxel> _rightVoxels)
            {
                sideCount = _sideCount;
                indexes = _indexes;
                keys = _keys; values = _values;
                backIndexes = _backIndexes;backValues = _backVoxels;
                leftIndexes = _leftIndexes;leftValues = _leftVoxels;
                forwardIndexes = _forwardIndexes;forwardValues = _forwardVoxels;
                rightIndexes = _rightIndexes;rightValues = _rightVoxels;
            }
            
            ChunkVoxel GetVoxel(Int3 _voxelID)
            {
                var curIndexes = indexes;
                var curVoxels = values;
                if (_voxelID.x < 0)
                {
                    curIndexes = leftIndexes;
                    curVoxels = leftValues;
                }

                if (_voxelID.x >= DVoxel.kChunkSize)
                {
                    curIndexes = rightIndexes;
                    curVoxels = rightValues;
                }

                if (_voxelID.z < 0)
                {
                    curIndexes = backIndexes;
                    curVoxels = backValues;
                }

                if (_voxelID.z >= DVoxel.kChunkSize)
                {
                    curIndexes = forwardIndexes;
                    curVoxels = forwardValues;
                }

                if(curIndexes.TryGetValue((_voxelID + DVoxel.kChunkSize) % DVoxel.kChunkSize,out var index))
                    return curVoxels[index];
                return ChunkVoxel.kInvalid;
            }
            
            public void Execute(int _jobIndex)
            {
                // var startIndex = _jobIndex * kProcessPerJob;
                // if (startIndex >= curKeys.Length)
                //     return;
                //
                // var count = math.min((_jobIndex + 1) * kProcessPerJob,curKeys.Length);
                int length = values.Length;
                for(int keyIndex=0;keyIndex<length;keyIndex++)
                {
                    var identity = keys[keyIndex];
                    var index = indexes[identity];
                    ChunkVoxel srcVoxel = values[index];
                    ChunkVoxel dstVoxel = new ChunkVoxel() {identity = srcVoxel.identity, type = srcVoxel.type};
                    for (int i = 0; i < 6; i++)
                    {
                        var facingVoxel = GetVoxel(identity + UCube.GetCubeOffset(UCube.IndexToFacing(i)));
                        bool sideValid =  facingVoxel.type != EVoxelType.Air;
                        dstVoxel.sideGeometry += (byte)((sideValid?1:0) << i);
                        if(!sideValid)
                            sideCount[0] += 1;
                    }

                    if(dstVoxel.sideGeometry== byte.MinValue)
                        continue;
                    for (int i = 0; i < 8; i++)
                    {
                        var cornerVoxel =  GetVoxel(identity+KCube.kCornerIdentity[i]);
                        bool cornerValid = cornerVoxel.type != EVoxelType.Air;
                        if (!cornerValid)
                            continue;
                        dstVoxel.cornerGeometry += (byte) (1 << i);
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        var intervalVoxel =  GetVoxel(identity + KCube.kIntervalIdentity[i]);
                        bool interValid = intervalVoxel.type != EVoxelType.Air;
                        var intervalByte = (interValid ? 1 : 0) << i;
                        dstVoxel.intervalGeometry += (ushort)intervalByte;
                    }
                
                    values[index] = dstVoxel;
                }
            }
        }
        
        public void PopulateVertices()
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            int vertexCount = m_SideCount * 4;
            int indexCount = m_SideCount * 6;
            
            new ExplicitJob(meshData, m_Values,vertexCount,indexCount).ScheduleParallel(1,1,default).Complete();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,new SubMeshDescriptor(0,m_SideCount*6){vertexCount = m_SideCount*4});

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
            
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> voxels;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<Vertex> vertices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<uint3> indexes;

            public ExplicitJob(Mesh.MeshData _meshData, NativeList<ChunkVoxel> _voxels,int _vertexCount,int _indexCount)
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
                    if (voxel.sideGeometry == byte.MinValue)
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
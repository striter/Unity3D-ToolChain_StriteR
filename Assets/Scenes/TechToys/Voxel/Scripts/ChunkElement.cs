using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Runtime.Geometry;
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

    [Flags]
    public enum EDirtyVoxels
    {
        Nothing = 0,
        Back = 1 << 0,
        Left = 1 << 1,
        Forward = 1 << 2,        
        Right =  1 << 3,
        Everything = int.MaxValue,
    }


    public class ChunkElement : PoolBehaviour<Int2>
    {
        public EChunkDirty m_Status { get; private set; }
        private MeshFilter m_Filter;
        private Mesh m_Mesh;

        private NativeHashMap<Int3, int> m_Indexes;
        private NativeList<Int3> m_Keys;
        private NativeList<ChunkVoxel> m_Values;
        private EDirtyVoxels m_DirtyVoxels;
        private int m_SideCount;

        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
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
        
        public override void OnPoolSpawn()
        {
            base.OnPoolSpawn();
            transform.position = DVoxel.GetChunkPositionWS(identity);
            m_Status = EChunkDirty.Generation;
            m_DirtyVoxels = EDirtyVoxels.Nothing;
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
                SetDirty(EDirtyVoxels.Everything);
                _chunks.GetValueOrDefault(identity + Int2.kForward)?.SetDirty(EDirtyVoxels.Back);
                _chunks.GetValueOrDefault(identity + Int2.kBack)?.SetDirty(EDirtyVoxels.Forward);
                _chunks.GetValueOrDefault(identity + Int2.kLeft)?.SetDirty(EDirtyVoxels.Right);
                _chunks.GetValueOrDefault(identity + Int2.kRight)?.SetDirty(EDirtyVoxels.Left);
                return;
            }

            if (m_Status.IsFlagEnable(EChunkDirty.Relation))
            {
                PopulateRelation(_chunks);
                m_Status &= int.MaxValue - EChunkDirty.Relation;
                return;
            }
            
            if (m_Status.IsFlagEnable(EChunkDirty.Vertices))
            {
                PopulateVertices();
                m_Status &= int.MaxValue - EChunkDirty.Vertices;
                return;
            }
        }

        void SetDirty(EDirtyVoxels _voxels)
        {
            m_DirtyVoxels |= _voxels;
            m_Status |= EChunkDirty.Relation | EChunkDirty.Vertices;
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
            new ImplicitJob(identity,m_Indexes,m_Keys,m_Values).ScheduleParallel(1,1,default).Complete();
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ImplicitJob : IJobFor
        {
            private TerrainData terrainData;
            private Int2 identity;
            private int startIndex;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3, int> indexes;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<Int3> keys;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> values;
            public ImplicitJob(Int2 _identity,NativeHashMap<Int3, int> _indexes,NativeList<Int3> _keys,NativeList<ChunkVoxel> _values)
            {
                terrainData = ChunkManager.Instance;
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
                
                float noise = UNoise.Perlin.Unit1f3(p3D * terrainData.caveScale);
                return noise > terrainData.caveValidation;
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
                    float formRandom = UNoise.Perlin.Unit1f2(p2D / terrainData.formScale) * .5f + .5f;
                    if (formRandom > terrainData.mountainValidation)
                        terrainForm = ETerrainForm.Mountains;

                    Insert(new Int3(i, 0, j), EVoxelType.BedRock);
                    int surfaceHeight = terrainData.baseHeight;
                    float noiseRandom = UNoise.Value.Unit1f2(p2D-.5f);
                    switch (terrainForm)
                    {
                        case ETerrainForm.Mountains:
                        {
                            float mountainFormStrength = umath.invLerp(terrainData.mountainValidation,1f,formRandom);
                            float terrainRandom = UNoise.Perlin.Unit1f2(p2D * terrainData.planeScale);
                            float mountainRandom = UNoise.Perlin.Unit1f2(p2D * terrainData.mountainScale) *.5f + .5f;

                            var height = math.max(terrainData.mountainHeight.GetValueContains(mountainRandom * mountainFormStrength), terrainData.planeHeight.GetValueContains(terrainRandom));
                            surfaceHeight +=  height;
                            surfaceHeight += 1;
                            
                            bool snow = surfaceHeight >=  terrainData.mountainForm;
                            
                            for (int k = 1; k <= surfaceHeight; k++)
                            {
                                EVoxelType type = EVoxelType.Stone;
                                if (snow && k == surfaceHeight)
                                    type = EVoxelType.Snow;
                            
                                if (CaveValidation(p2D,k))
                                    continue;
                                Insert(new Int3(i,k,j),type);
                            }
                        }
                            break;
                        case ETerrainForm.Plane:
                        {
                            float planeRandom = UNoise.Perlin.Unit1f2(p2D * terrainData.planeScale);
                            int dirtRandom = terrainData.dirtRandom.GetValueContains(noiseRandom);
                            surfaceHeight += terrainData.planeHeight.GetValueContains(planeRandom) - dirtRandom;
                            int stoneHeight = surfaceHeight;
                            surfaceHeight += dirtRandom;
                            int dirtHeight = surfaceHeight;
                            surfaceHeight += 1;

                            for (int k = 1; k <= surfaceHeight; k++)
                            {
                                EVoxelType type;
                                if(k <= stoneHeight)
                                    type = EVoxelType.Stone;
                                else if (k <= dirtHeight)
                                    type = EVoxelType.Dirt;
                                else
                                    type = EVoxelType.Grass;
                            
                                if (CaveValidation(p2D,k))
                                    continue;
                                Insert(new Int3(i,k,j),type);
                            }
                        }
                        break;
                    }
                }
            }
        }

        void PopulateRelation(Dictionary<Int2,ChunkElement> _chunks)
        {
            var back = _chunks.TryGetValue(identity + Int2.kBack, out var backChunk);
            var left = _chunks.TryGetValue(identity + Int2.kLeft, out var leftChunk);
            var forward = _chunks.TryGetValue(identity + Int2.kForward, out var forwardChunk);
            var right = _chunks.TryGetValue(identity + Int2.kRight, out var rightChunk);
            
            var emptyIndexes = new NativeHashMap<Int3, int>(0,Allocator.Persistent);
            var emptyValues = new NativeList<ChunkVoxel>(0, Allocator.Persistent);
        
            NativeArray<int> sideCount = new NativeArray<int>(1,Allocator.TempJob);
            var refreshJob = new RefreshRelationJob(sideCount,m_DirtyVoxels,m_Indexes,m_Keys,m_Values,
                back?backChunk.m_Indexes : emptyIndexes,back?backChunk.m_Values : emptyValues,
                left?leftChunk.m_Indexes : emptyIndexes,left?leftChunk.m_Values : emptyValues,
                forward?forwardChunk.m_Indexes:emptyIndexes, forward?forwardChunk.m_Values : emptyValues,
                right?rightChunk.m_Indexes : emptyIndexes,right?rightChunk.m_Values : emptyValues);
            refreshJob.ScheduleParallel(1,1,default).Complete();
            m_SideCount = sideCount[0];

            sideCount.Dispose();
            
            emptyIndexes.Dispose();
            emptyValues.Dispose();
            m_DirtyVoxels = EDirtyVoxels.Nothing;
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct RefreshRelationJob : IJobFor
        {
            private int dirtyVoxels;
            [NativeDisableContainerSafetyRestriction] private NativeArray<int> sideCount;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<Int3> keys;
            [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> values;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeHashMap<Int3, int> indexes,backIndexes,leftIndexes,forwardIndexes,rightIndexes;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<ChunkVoxel> backValues,leftValues,forwardValues,rightValues;
            
            public RefreshRelationJob(NativeArray<int> _sideCount,EDirtyVoxels _dirtyVoxels, NativeHashMap<Int3,int> _indexes,NativeList<Int3> _keys,NativeList<ChunkVoxel> _values,
                NativeHashMap<Int3,int> _backIndexes,NativeList<ChunkVoxel> _backVoxels,
                NativeHashMap<Int3,int> _leftIndexes,NativeList<ChunkVoxel> _leftVoxels,
                NativeHashMap<Int3,int> _forwardIndexes,NativeList<ChunkVoxel> _forwardVoxels,
                NativeHashMap<Int3,int> _rightIndexes,NativeList<ChunkVoxel> _rightVoxels)
            {
                dirtyVoxels = (int)_dirtyVoxels;
                sideCount = _sideCount;
                indexes = _indexes;
                keys = _keys; values = _values;
                backIndexes = _backIndexes;backValues = _backVoxels;
                leftIndexes = _leftIndexes;leftValues = _leftVoxels;
                forwardIndexes = _forwardIndexes;forwardValues = _forwardVoxels;
                rightIndexes = _rightIndexes;rightValues = _rightVoxels;
            }

            bool IsVoxelDirty(Int3 _voxelID)
            {
                if (dirtyVoxels == int.MaxValue)
                    return true;
                
                if ((dirtyVoxels & 1) == 1 && _voxelID.y == 0)
                    return true;
                if ((dirtyVoxels & 2) == 2 && _voxelID.y == DVoxel.kChunkSizeM1)
                    return true;
                if ((dirtyVoxels & 4) == 4 && _voxelID.x == 0)
                    return true;
                if ((dirtyVoxels & 8) == 8 && _voxelID.x == DVoxel.kChunkSizeM1)
                    return true;
                return false;
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
                int length = values.Length;
                for(int keyIndex=0;keyIndex<length;keyIndex++)
                {
                    var identity = keys[keyIndex];
                    var index = indexes[identity];
                    ChunkVoxel srcVoxel = values[index];
                    
                    if (!IsVoxelDirty(identity))
                    {
                        sideCount[0] += (6-UByte.PosValidCount(srcVoxel.sideGeometry));
                        continue;
                    }

                    ChunkVoxel replaceVoxel = new ChunkVoxel() {identity = srcVoxel.identity, type = srcVoxel.type};
                    for (int i = 0; i < 6; i++)
                    {
                        var facingVoxel = GetVoxel(identity + UCube.GetCubeOffset(UCube.IndexToFacing(i)));
                        bool sideValid =  facingVoxel.type != EVoxelType.Air;
                        replaceVoxel.sideGeometry += (byte)((sideValid?1:0) << i);
                        if(!sideValid)
                            sideCount[0] += 1;
                    }

                    if(replaceVoxel.sideGeometry== byte.MinValue)
                        continue;
                    
                    for (int i = 0; i < 8; i++)
                    {
                        var cornerVoxel =  GetVoxel(identity+KCube.kCornerIdentity[i]);
                        bool cornerValid = cornerVoxel.type != EVoxelType.Air;
                        if (!cornerValid)
                            continue;
                        replaceVoxel.cornerGeometry += (byte) (1 << i);
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        var intervalVoxel =  GetVoxel(identity + KCube.kIntervalIdentity[i]);
                        bool interValid = intervalVoxel.type != EVoxelType.Air;
                        var intervalByte = (interValid ? 1 : 0) << i;
                        replaceVoxel.intervalGeometry += (ushort)intervalByte;
                    }
                
                    values[index] = replaceVoxel;
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

            m_Mesh.bounds = UBoundsIncrement.MinMax(Vector3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkSize));
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
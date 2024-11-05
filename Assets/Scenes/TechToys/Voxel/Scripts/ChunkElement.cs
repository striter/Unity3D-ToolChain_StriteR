using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class ChunkElement : PoolBehaviour<Int2>
    {
        [Readonly] public int m_SideCount;
        public EChunkDirty m_DirtyStatus;
        private MeshFilter m_Filter;
        private Mesh m_Mesh;

        private NativeHashMap<Int3, int> m_Indexes;
        private NativeList<Int3> m_Keys;
        private NativeList<ChunkVoxel> m_Values;

        static readonly float3 kChunkSize = new float3(DVoxel.kVoxelSize* DVoxel.kChunkSize);
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
            m_DirtyStatus = (EChunkDirty)int.MaxValue;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            Clear();
        }

        public bool Tick(float _deltaTime, Dictionary<Int2, ChunkElement> _chunks)
        {
            if (m_DirtyStatus.IsFlagEnable(EChunkDirty.Generation))
            {
                m_DirtyStatus &= int.MaxValue - EChunkDirty.Generation;
                Clear();
                PopulateImplicit();
                return true;
            }

            if (!m_DirtyStatus.IsFlagEnable(EChunkDirty.Relation) && !m_DirtyStatus.IsFlagEnable(EChunkDirty.Vertices))
                return false;
            
            var sides = new Quad<ChunkElement>(
                _chunks.TryGetValue(identity + Int2.kBack, out var backChunk) ? backChunk : null,
                _chunks.TryGetValue(identity + Int2.kLeft, out var leftChunk) ? leftChunk : null,
                _chunks.TryGetValue(identity + Int2.kForward, out var forwardChunk) ? forwardChunk : null,
                _chunks.TryGetValue(identity + Int2.kRight, out var rightChunk) ? rightChunk : null);

            if (sides.Any(p => p == null))
                return false;
            
            if (m_DirtyStatus.IsFlagEnable(EChunkDirty.Relation))
            {
                if (sides.Any(p => p.m_DirtyStatus.IsFlagEnable(EChunkDirty.Generation)))
                    return false;
                
                if (!PopulateRelation(sides))
                    return false;
                m_DirtyStatus &= int.MaxValue - EChunkDirty.Relation;
                return true;
            }
            
            if (m_DirtyStatus.IsFlagEnable(EChunkDirty.Vertices))
            {
                PopulateVertices();
                m_DirtyStatus &= int.MaxValue - EChunkDirty.Vertices;
                return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var index = 0;
            foreach (var chunkDirty in UEnum.GetEnums<EChunkDirty>())
            {
                if (!m_DirtyStatus.IsFlagEnable(chunkDirty))
                    continue;
                Gizmos.color = UColor.IndexToColor(index++);

                GBox.Minmax(float3.zero, kChunkSize).DrawGizmos();
            }
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
                
                float noise = Noise.Perlin.Unit1f3(p3D * terrainData.caveScale);
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
                    float formRandom = Noise.Perlin.Unit1f2(p2D / terrainData.formScale) * .5f + .5f;
                    if (formRandom > terrainData.mountainValidation)
                        terrainForm = ETerrainForm.Mountains;

                    Insert(new Int3(i, 0, j), EVoxelType.BedRock);
                    int surfaceHeight = terrainData.baseHeight;
                    float noiseRandom = Noise.Value.Unit1f2(p2D-.5f);
                    switch (terrainForm)
                    {
                        case ETerrainForm.Mountains:
                        {
                            float mountainFormStrength = umath.invLerp(terrainData.mountainValidation,1f,formRandom);
                            float terrainRandom = Noise.Perlin.Unit1f2(p2D * terrainData.planeScale);
                            float mountainRandom = Noise.Perlin.Unit1f2(p2D * terrainData.mountainScale) *.5f + .5f;

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
                            float planeRandom = Noise.Perlin.Unit1f2(p2D * terrainData.planeScale);
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

        bool PopulateRelation(Quad<ChunkElement> _sides)
        {
            var back = _sides.B;
            var left = _sides.L;
            var forward = _sides.F;
            var right = _sides.R;
            
            var emptyIndexes = new NativeHashMap<Int3, int>(0,Allocator.Persistent);
            var emptyValues = new NativeList<ChunkVoxel>(0, Allocator.Persistent);
        
            NativeArray<int> sideCount = new NativeArray<int>(1,Allocator.TempJob);
            var refreshJob = new RefreshRelationJob(sideCount,m_Indexes,m_Keys,m_Values,
                back?back.m_Indexes : emptyIndexes,back?back.m_Values : emptyValues,
                left?left.m_Indexes : emptyIndexes,left?left.m_Values : emptyValues,
                forward?forward.m_Indexes:emptyIndexes, forward?forward.m_Values : emptyValues,
                right?right.m_Indexes : emptyIndexes,right?right.m_Values : emptyValues);
            refreshJob.ScheduleParallel(1,1,default).Complete();
            m_SideCount = sideCount[0];

            sideCount.Dispose();
            
            emptyIndexes.Dispose();
            emptyValues.Dispose();
            return true;
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct RefreshRelationJob : IJobFor
        {
            [NativeDisableContainerSafetyRestriction] private NativeArray<int> sideCount;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<Int3> keys;
            [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> values;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeHashMap<Int3, int> indexes,backIndexes,leftIndexes,forwardIndexes,rightIndexes;
            [NativeDisableContainerSafetyRestriction] [ReadOnly] private NativeList<ChunkVoxel> backValues,leftValues,forwardValues,rightValues;
            
            public RefreshRelationJob(NativeArray<int> _sideCount,NativeHashMap<Int3,int> _indexes,NativeList<Int3> _keys,NativeList<ChunkVoxel> _values,
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
                if(_voxelID.y < 0)
                    return ChunkVoxel.kVoid;
                
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

                _voxelID.x = (_voxelID.x + DVoxel.kChunkSize) % DVoxel.kChunkSize;
                _voxelID.z = (_voxelID.z + DVoxel.kChunkSize) % DVoxel.kChunkSize;

                if(curIndexes.TryGetValue(_voxelID,out var index))
                    return curVoxels[index];
                return ChunkVoxel.kInvalid;
            }
            
            public void Execute(int _jobIndex)
            {
                var length = values.Length;
                var renderFaces = 0;
                for(var keyIndex=0;keyIndex<length;keyIndex++)
                {
                    var identity = keys[keyIndex];
                    var index = indexes[identity];
                    var faceToRender = 0;
                    var srcVoxel = values[index];
                    var replaceVoxel = new ChunkVoxel() {identity = srcVoxel.identity, type = srcVoxel.type};
                    for (var i = 0; i < 6; i++)
                    {
                        var facing = UCube.IndexToFacing(i);
                        var facingVoxel = GetVoxel(identity + UCube.GetCubeOffset(facing));
                        var shouldRenderFace = facingVoxel.type == EVoxelType.Air;
                        if(shouldRenderFace)
                            faceToRender += 1;
                        else
                            replaceVoxel.sideGeometry += (byte)(1 << i);
                    }

                    if (faceToRender != 6)
                        renderFaces += faceToRender;

                    if (replaceVoxel.sideGeometry != ChunkVoxel.kEmptyGeometry && replaceVoxel.sideGeometry != ChunkVoxel.kFullGeometry)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            var cornerVoxel =  GetVoxel(identity+KCube.kCornerIdentity[i]);
                            var cornerValid = cornerVoxel.type != EVoxelType.Air;
                            if (!cornerValid)
                                continue;
                            replaceVoxel.cornerGeometry += (byte) (1 << i);
                        }

                        for (var i = 0; i < 12; i++)
                        {
                            var intervalVoxel =  GetVoxel(identity + KCube.kIntervalIdentity[i]);
                            var interValid = intervalVoxel.type != EVoxelType.Air;
                            var intervalByte = (interValid ? 1 : 0) << i;
                            replaceVoxel.intervalGeometry += (ushort)intervalByte;
                        }

                    }
                    
                    values[index] = replaceVoxel;
                }

                sideCount[0] = renderFaces;
            }
        }
        
        public void PopulateVertices()
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            var vertexCount = m_SideCount * 4;
            var indexCount = m_SideCount * 6;
            
            var bounds = new NativeArray<float3>(1, Allocator.TempJob);
            var meshjob = new ExplicitMeshJob(meshData, m_Values,bounds,vertexCount,indexCount);
            meshjob.ScheduleParallel(1,1,default).Complete();

            try
            {
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){ vertexCount = vertexCount, });
                m_Mesh.bounds = GBox.Minmax(0, bounds[0]);
                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh);
            }
            catch (Exception e)
            {
                int length = m_Values.Length;

                var sideCount = 0;
                for (var i = 0; i < length; i++)
                {
                    var voxel = m_Values[i];
                    if (voxel.sideGeometry == ChunkVoxel.kEmptyGeometry || voxel.sideGeometry == ChunkVoxel.kFullGeometry)
                        continue;
                    sideCount++;
                }
                
                Debug.LogError($"Chunk Error {identity} {sideCount}/{m_SideCount} \n{e.Message}\n{e.StackTrace}");
                meshDataArray.Dispose();
            }

            bounds.Dispose();
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct ExplicitMeshJob : IJobFor
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

            private NativeArray<float3> bounds;
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeList<ChunkVoxel> voxels;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<Vertex> vertices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<uint3> indexes;

            public ExplicitMeshJob(Mesh.MeshData _meshData, NativeList<ChunkVoxel> _voxels,NativeArray<float3> _bounds,int _vertexCount,int _indexCount)
            {
                voxels = _voxels;
                bounds = _bounds;
                
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

                int highestID = 0;
                int vertexIndex = 0;
                int triangleIndex = 0;
                for (int i = 0; i < length; i++)
                {
                    var voxel = voxels[i];
                    if (voxel.sideGeometry == ChunkVoxel.kEmptyGeometry || voxel.sideGeometry == ChunkVoxel.kFullGeometry)
                        continue;

                    highestID = math.max(highestID, voxel.identity.y);
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

                bounds[0] = new float3(DVoxel.kVoxelSize * DVoxel.kChunkSize).setY(highestID * DVoxel.kVoxelSize);
            }
        }

    }
}
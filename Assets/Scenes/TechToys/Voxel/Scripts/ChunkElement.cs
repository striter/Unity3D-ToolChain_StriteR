using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Runtime.InteropServices;
using Runtime.Geometry;
using TheVoxel.ChunkProcess;
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

    [Serializable]
    public struct ChunkData
    {
        public GBox bounds;
        public int sideCount;
        public int highestCount;

        public static ChunkData kDefault = new ChunkData()
        {
            bounds = GBox.Minmax(float3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkSize)),
            sideCount = 0,
            highestCount = 0,
        };
    }
    
    public class ChunkElement : PoolBehaviour<Int2>
    {
        [Readonly] public ChunkData m_Data;
        public EChunkDirty m_DirtyStatus;
        private MeshFilter m_Filter;
        private Mesh m_Mesh;

        private NativeHashMap<Int3, ChunkVoxel> m_Indexes;

        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            m_Filter = GetComponent<MeshFilter>();
            m_Mesh = new Mesh(){name = "Chunk Mesh",hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            
            m_Filter.sharedMesh = m_Mesh;
            m_Indexes = new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.Persistent);
            m_Data = ChunkData.kDefault;
        }

        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            m_Indexes.Dispose();
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

        public bool m_GizmosDetailed;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var index = 0;
            foreach (var chunkDirty in UEnum.GetEnums<EChunkDirty>())
            {
                if (!m_DirtyStatus.IsFlagEnable(chunkDirty))
                    continue;
                Gizmos.color = UColor.IndexToColor(index++);

                m_Data.bounds.DrawGizmos();
                switch (chunkDirty)
                {
                    case EChunkDirty.Generation:
                        break;
                    case EChunkDirty.Relation:
                        break;
                    case EChunkDirty.Vertices:
                        break;
                }
            }
        }

        void Clear()
        {
            m_Mesh.Clear();
            m_Indexes.Clear();
        }

        void PopulateImplicit()
        {
            var implicitJob =new ImplicitJob(identity,m_Indexes);
            implicitJob.ScheduleParallel(1,1,default).Complete();
            var length = m_Indexes.Count;
            var keys = m_Indexes.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < length; i++)
                m_Data.highestCount = Math.Max(m_Data.highestCount, m_Indexes[keys[i]].identity.y);
            m_Data.bounds = GBox.Minmax(float3.zero, new float3(DVoxel.kVoxelSize * DVoxel.kChunkSize).setY(m_Data.highestCount * DVoxel.kVoxelSize));
        }


        bool PopulateRelation(Quad<ChunkElement> _sides)
        {
            var back = _sides.B;
            var left = _sides.L;
            var forward = _sides.F;
            var right = _sides.R;
            
            var emptyIndexes = new NativeHashMap<Int3, ChunkVoxel>(0,Allocator.Persistent);
        
            NativeArray<int> sideCount = new NativeArray<int>(1,Allocator.TempJob);
            var refreshJob = new RefreshRelationJob(sideCount,m_Indexes,
               back?back.m_Indexes : emptyIndexes,
                left?left.m_Indexes : emptyIndexes,
                forward?forward.m_Indexes : emptyIndexes,
                right?right.m_Indexes : emptyIndexes);
            refreshJob.ScheduleParallel(1,1,default).Complete();
            m_Data.sideCount = sideCount[0];

            sideCount.Dispose();
            
            emptyIndexes.Dispose();
            return true;
        }

        public void PopulateVertices()
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            var vertexCount = m_Data.sideCount * 4;
            var indexCount = m_Data.sideCount * 6;
            
            var meshjob = new ExplicitMeshJob(meshData, m_Indexes,vertexCount,indexCount);
            meshjob.ScheduleParallel(1,1,default).Complete();

            try
            {
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(0,new SubMeshDescriptor(0,indexCount){ vertexCount = vertexCount, });
                m_Mesh.bounds = m_Data.bounds;
                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,m_Mesh);
            }
            catch (Exception e)
            {
                int length = m_Indexes.Count;

                var keys = m_Indexes.GetKeyArray(Allocator.Temp);
                var sideCount = 0;
                for (var i = 0; i < length; i++)
                {
                    var voxel = m_Indexes[keys[i]];
                    if (voxel.sideGeometry == ChunkVoxel.kEmptyGeometry || voxel.sideGeometry == ChunkVoxel.kFullGeometry)
                        continue;
                    sideCount++;
                }

                keys.Dispose();
                Debug.LogError($"Chunk Error {identity} {sideCount}/{m_Data.sideCount} \n{e.Message}\n{e.StackTrace}");
                meshDataArray.Dispose();
                
            }
        }

    }
}
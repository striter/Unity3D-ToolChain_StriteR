using System;
using System.Collections.Generic;
using Geometry;
using MeshFragment;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module.Cluster
{
    public static class UModuleClusterByte
    {
        private static readonly Dictionary<EClusterStatus, OrientedModuleIndexer[]> kClusterUnitIndexes;
        public static OrientedModuleIndexer GetOrientedClusterUnitIndex(byte _srcByte,EClusterStatus _status)=> kClusterUnitIndexes[_status][_srcByte];
        static UModuleClusterByte()
        {
            kClusterUnitIndexes = new Dictionary<EClusterStatus, OrientedModuleIndexer[]>();
            foreach (var status in UEnum.GetEnums<EClusterStatus>())
                kClusterUnitIndexes.Add(status,IterateAllClusterUnits(status));
        }

        static OrientedModuleIndexer[] IterateAllClusterUnits(EClusterStatus _status)   //256 * 8
        {
            var indexer = new OrientedModuleIndexer[byte.MaxValue + 1];
            TSPoolList<byte>.Spawn(out var validModule);
            TSPoolHashset<byte>.Spawn(out var iteratedByte);
            for(int i=0;i<=byte.MaxValue;i++)
                indexer[i]=OrientedModuleIndexer.Invalid;
            
            var byteQubeIndexer = UModuleByte.kByteQubeIndexer;
            
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var byteQubes = byteQubeIndexer[(byte)i];
                for (int j = 0; j < 8; j++)
                {
                    var srcByte = byteQubes[j];
                    if (iteratedByte.Contains(srcByte)) 
                        continue;
                    iteratedByte.Add(srcByte);

                    if (!DModuleCluster.IsValidClusterUnit(_status,srcByte))
                        continue;
                    
                    var orientedByte =UModuleByte.kByteOrientation[srcByte];
                    var moduleIndex = validModule.FindIndex(p => p == orientedByte._byte);
                    if (moduleIndex == -1)
                    {
                        moduleIndex = validModule.Count;
                        validModule.Add(srcByte);
                    }
                    
                    indexer[srcByte] = new OrientedModuleIndexer(){srcByte = orientedByte._byte ,index = moduleIndex,orientation = orientedByte._orientation};
                }
            }
            
            TSPoolList<byte>.Recycle(validModule);
            TSPoolHashset<byte>.Recycle(iteratedByte);
            return indexer;
        }
        public static IEnumerable<byte> IterateClusterBytes(EClusterStatus _status)=> UModuleByte.IterateClusterBytes(kClusterUnitIndexes[_status]);

        public static ModuleClusterCornerData GetOrientedClusterUnitIndex(this ModuleCollection _collection,ModuleClusterInputData _input)
        {
            if (!_input.valid)
                return ModuleClusterCornerData.kInvalid;

            var moduleIndexes = GetOrientedClusterUnitIndex(_input.anchorByte, _input.status);
            if (moduleIndexes.index == -1)
                return ModuleClusterCornerData.kInvalid;
            
            ref var clusterData = ref _collection.m_ModuleLibrary[_input.type];
            ref var moduleMeshes = ref clusterData.m_ClusterData[UEnum.GetIndex(_input.status)];
            
            if (moduleIndexes.index >= moduleMeshes.m_Units.Length)
                throw new Exception($"Invalid Module Mesh Length! {clusterData.name} {_input.status}");

            var orientation = moduleIndexes.orientation;
            var index = moduleIndexes.index;
            
            ref var clusterUnitData = ref moduleMeshes.m_Units[moduleIndexes.index];
            var mixableModuleIndexes = UModuleByte.kByteOrientation[(byte)( _input.anchorByte | _input.relationByte)];

            var possibilityIndex = clusterUnitData.m_Possibilities.Length-1;
            for (int i = 0; i < clusterUnitData.m_Possibilities.Length -1; i++)
            {
                ref var curPossibility = ref clusterUnitData.m_Possibilities[i];
                var possibilityModuleIndexes = UModuleByte.kByteOrientation[curPossibility.m_MixableReadMask];
                if ( mixableModuleIndexes._byte == possibilityModuleIndexes._byte)
                {
                    possibilityIndex = i;
                    orientation = orientation==0?  (mixableModuleIndexes._orientation - possibilityModuleIndexes._orientation ) :orientation;
                    break;
                }
            }

            return new ModuleClusterCornerData {
                index = index,
                orientation = orientation,
                possibility = possibilityIndex,
            };
        }
        
        public static FMeshFragmentCluster GetClusterData(this ModuleCollection _collection,ModuleClusterInputData _input,ModuleClusterCornerData _data)=>
            _collection[_input.type].m_ClusterData[UEnum.GetIndex(_input.status)].m_Units[_data.index].m_Possibilities[_data.possibility].m_Mesh;       //?
    }
    
}
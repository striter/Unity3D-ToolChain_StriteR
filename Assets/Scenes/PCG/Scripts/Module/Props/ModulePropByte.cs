using System;
using System.Collections.Generic;
using System.ComponentModel;
using MeshFragment;
using PCG.Module.Cluster;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module.Prop
{
    public static class UModulePropByte
    {
        private static readonly OrientedModuleIndexer[] kPathIndexer;
        private static readonly OrientedModuleIndexer[] kCommonDecorationIndexer;
        private static readonly OrientedModuleIndexer[] kSurfaceDecorationIndexer;
        static UModulePropByte()
        {
            kPathIndexer = GetOrientedModuleIndexes(1<<4,DModuleProp.IsValidPath,UModuleByte.kQuadOrientation);
            kCommonDecorationIndexer = GetOrientedModuleIndexes(byte.MaxValue+1, p => DModuleProp.IsValidDecoration(true, p), UModuleByte.kByteOrientation);
            kSurfaceDecorationIndexer = GetOrientedModuleIndexes(byte.MaxValue+1, p => DModuleProp.IsValidDecoration(false, p), UModuleByte.kByteOrientation);
        }

        static OrientedModuleIndexer[] GetOrientedModuleIndexes(int _count,Predicate<byte> _validCheck,(byte _byte,int _orientation)[] _byteOrientations)
        {
            OrientedModuleIndexer[] indexer = new OrientedModuleIndexer[_count];
            TSPoolList<byte>.Spawn(out var validModule);
            TSPoolHashset<byte>.Spawn(out var iteratedBytes);
            
            for (int i = 0; i < _count; i++)
            {
                var qubeByte = (byte)i;
                if (!_validCheck(qubeByte))
                {
                    indexer[i]=OrientedModuleIndexer.Invalid;
                    continue;
                }
                var orientedByte = _byteOrientations[i];
                var moduleIndex = validModule.FindIndex(p => p == orientedByte._byte);
                if (moduleIndex == -1)
                {
                    moduleIndex = validModule.Count;
                    validModule.Add(qubeByte);
                }
                indexer[i] = new OrientedModuleIndexer() { srcByte = orientedByte._byte, index = moduleIndex ,orientation = orientedByte._orientation};
            }
            TSPoolList<byte>.Recycle(validModule);
            TSPoolHashset<byte>.Recycle(iteratedBytes);
            return indexer;
        }
        public static IEnumerable<byte> IteratePathBytes() => UModuleByte.IterateClusterBytes(kPathIndexer);

        public static IEnumerable<byte> IterateDecorationBytes(EClusterType _clusterType)=> UModuleByte.IterateClusterBytes(GetOrientedPropIndexes(_clusterType));

        public static OrientedModuleIndexer GetOrientedPathIndex(byte _pathByte)=>kPathIndexer[_pathByte];
        public static bool GetOrientedPath(this ModulePathData _data,byte _quadByte,out FMeshFragmentCluster _pathData,out int _orientation)
        {
            _pathData = default;
            _orientation = 0;

            var quadIndexer = kPathIndexer[_quadByte];
            if (quadIndexer.index == -1)
            {
                Debug.LogError($"Invalid Path Byte:{_quadByte}");
                return false;
            }
            
            if ( quadIndexer.index>=_data.m_Units.Length)
            {
                Debug.LogError("Invalid Module Mesh Length!"+_quadByte);
                return false;
            }
            
            _pathData =  _data.m_Units[quadIndexer.index];
            _orientation = quadIndexer.orientation;
            return true;
        }

        public static OrientedModuleIndexer[] GetOrientedPropIndexes(EClusterType _clusterType)
        {
            switch (_clusterType)
            {
                default: throw new InvalidEnumArgumentException();
                case EClusterType.Vanilla:
                    return kCommonDecorationIndexer;
                case EClusterType.Foundation:
                case EClusterType.Surface:
                    return kSurfaceDecorationIndexer;
            }
        }
        public static OrientedModuleIndexer GetOrientedPropIndex(EClusterType _clusterType,byte _clusterByte)=>GetOrientedPropIndexes(_clusterType)[_clusterByte];


        public static bool GetOrientedProp(this ModuleDecorationSet _data,EClusterType _clusterType, byte _decorationByte, out ModulePossibilitySet _decorationData, out int _orientation)
        {
            _decorationData = default;
            _orientation = 0;
            var indexer = GetOrientedPropIndex(_clusterType,_decorationByte);
            if (indexer.index == -1)
                return false;
            
            if (indexer.index>=_data.propSets.Length)
            {
                Debug.LogError("Invalid Module Mesh Length!"+_decorationByte);
                return false;
            }

            _decorationData = _data.propSets[indexer.index];
            _orientation = indexer.orientation;
            return true;
        }
    }
}
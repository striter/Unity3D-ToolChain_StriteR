using System.Collections.Generic;
using Geometry;
using PCG.Module.Cluster;

namespace PCG.Module
{
    public static class DModule
    {
        public static ModuleCollection Collection;
        
        public static bool IsCornerAdjacent(int _srcIndex,int _dstIndex)=> _srcIndex == _dstIndex;
        public static Qube<PCGID> CollectVoxelCorners(this IVoxel _voxel) => _voxel.m_Quad.Quad.Corners(_voxel.Identity.height);
    }
}

using System.Collections.Generic;
using Runtime.Geometry;
using UnityEngine;
using TechToys.ThePlanet.Module.Cluster;

namespace TechToys.ThePlanet.Module
{
    public static class DModule
    {
        public static ModuleCollection Collection;
        public static Color[] EmissionColors;

        public const int kIDCluster = 0;
        public const int kIDPath = 1;
        public const int kIDProp = 2;

        
        public static bool IsCornerAdjacent(int _srcIndex,int _dstIndex)=> _srcIndex == _dstIndex;
        public static Qube<PCGID> CollectVoxelCorners(this IVoxel _voxel) => _voxel.m_Quad.Quad.Corners(_voxel.Identity.height);
    }
}

using System;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public enum EModuleType
    {
        Invalid=-1,
        Green,
        Red,
    }

    public struct ModuleQubeData
    {
        public EModuleType moduleType;
        public EPileStatus moduleStatus;
        public int moduleIndex;
        public int orientation;
    }
}
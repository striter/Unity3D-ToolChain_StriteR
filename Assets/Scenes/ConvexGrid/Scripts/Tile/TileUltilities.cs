using System.Collections.Generic;
using Geometry.Voxel;
using LinqExtentions;
using Procedural;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public enum EGridQuadGeometry
    {
        Full,
        Half,
    }

    public enum EGridVoxelGeometry
    {
        Plane,
        VoxelTight,
        VoxelTopBottom,
    }

    public static class UTile
    {
        public static Vector3 GetCornerPositionWS(this ConvexVertex _vertex,byte _height)
        {
            return _vertex.m_Coord.ToPosition() + GetCornerHeight(_height);
        }
        
        public static Vector3 GetCornerHeight(PileID _id)
        {
            return GetCornerHeight( _id.height);
        }

        public static Vector3 GetCornerHeight(byte _height)
        {
            return KConvexGrid.tileHeightHalfVector+KConvexGrid.tileHeightVector * _height;
        }

        public static Vector3 GetVoxelHeight(PileID _id)
        {
            return KConvexGrid.tileHeightVector * _id.height;
        }
    }
}
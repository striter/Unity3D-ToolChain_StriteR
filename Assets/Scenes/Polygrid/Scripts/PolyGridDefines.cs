using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry;
using Procedural;
using UnityEngine;

namespace PolyGrid
{
    public enum EQuadGeometry
    {
        Full,
        Half,
    }

    public enum EVoxelGeometry
    {
        Plane,
        VoxelTight,
        VoxelTopBottom,
    }

    public static class KPolyGrid
    {
        public const float tileSize = 4f;
        
        public const float tileHeight = tileSize;
        public const float tileHeightHalf = tileHeight / 2f;
        
        public static readonly Vector3 tileHeightVector = tileHeight*Vector3.up;
        public static readonly Vector3 tileHeightHalfVector = tileHeightHalf*Vector3.up;
    }

    public static class DPolyGrid
    {
        public static Vector3 GetCornerHeight(PileID _id)
        {
            return GetCornerHeight( _id.height);
        }

        public static Vector3 GetCornerHeight(byte _height)
        {
            return KPolyGrid.tileHeightHalfVector+KPolyGrid.tileHeightVector * _height;
        }

        public static Vector3 GetVoxelHeight(PileID _id)
        {
            return KPolyGrid.tileHeightVector * _id.height;
        }
        
        public static Vector3 GetCornerPositionWS(this PolyVertex _vertex,byte _height)
        {
            return _vertex.m_Coord.ToPosition() + GetCornerHeight(_height);
        }

        public static IEnumerable<PileID> AllNearbyCorner(this PolyVertex _vertex,byte _height)
        {
            if (_height != byte.MinValue)
                yield return new PileID(_vertex.m_Identity,UByte.BackOne(_height));
            foreach (var vertex in _vertex.m_NearbyVertex)
                yield return new PileID(vertex,_height);
            if (_height != byte.MaxValue)
                yield return new PileID(_vertex.m_Identity,UByte.ForwardOne(_height));
        }
        public static IEnumerable<PileID> AllNearbyVoxels(this PolyVertex _vertex,byte _height)
        {
            var nextHeight = UByte.ForwardOne(_height);
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                yield return new PileID(quadID, _height);
                yield return new PileID(quadID, nextHeight);
            }
        }
    }
}
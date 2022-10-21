using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;

namespace PCG
{
    using static PCGDefines<int>;
    public enum EQuadGeometry
    {
        Full,
        Half,
    }

    public static class KPCG
    {
        public static float kPolySize;
        public static float kPolyHeight;
        public static float kPolyHeightH;
        public static Vector3 kCornerHeightVector;
        public static Vector3 kCornerHeightVectorH;
        public static Vector3 kPolyScale;

        public static void Setup(float polySize, float polyHeight)
        {
            kPolySize = polySize;
            kPolyHeight = polyHeight;
            kPolyHeightH = kPolyHeight / 2f;
            kCornerHeightVector = kPolyHeight * Vector3.up;
            kCornerHeightVectorH = kPolyHeightH * Vector3.up;
            kPolyScale = new Vector3(kPolySize,kPolyHeight,kPolySize);
        }
        
        public static Vector3 GetPlaneHeight(byte _height) => kCornerHeightVector * _height;
        public static Vector3 GetCornerHeight(int _height)=> kCornerHeightVectorH+kCornerHeightVector * _height;
        public static Vector3 GetVoxelHeight(int _height)=>  kCornerHeightVector * _height;
    }

    public static class DPCG
    {
        public static Vector3 GetCornerHeight(this PCGID _voxelID) => KPCG.GetCornerHeight(_voxelID.height);
        public static Vector3 GetCornerPosition(this PolyVertex _vertex, byte _height)=> _vertex.m_Coord.ToPosition() + KPCG.GetCornerHeight(_height);
        public static Vector3 GetVoxelHeight(this PCGID _voxelID) => KPCG.GetVoxelHeight(_voxelID.height);
        public static Vector3 GetVoxelPosition(this PolyQuad _quad,byte _height) => _quad.m_CenterWS.ToPosition() + KPCG.GetVoxelHeight(_height);
        
        public static bool TryDownward(this PCGID _src, out PCGID _dst)
        {
            _dst = default;
            if (_src.height == byte.MinValue)
                return false;
            _dst = new PCGID(_src.location, UByte.BackOne(_src.height));
            return true;
        }
        public static bool TryUpward(this PCGID _src, out PCGID _dst)
        {
            _dst = default;
            if (_src.height == byte.MaxValue)
                return false;
            _dst= new PCGID(_src.location,UByte.ForwardOne(_src.height));
            return true;
        }
        public static bool TryUpward(this PCGID _src, out PCGID _dst,int _index)
        {
            _dst = default;
            var dstHeight = _src.height + _index;
            if (dstHeight >= byte.MaxValue)
                return false;
            _dst= new PCGID(_src.location, (byte)dstHeight);
            return true;
        }

        private static readonly List<PCGID> kIdentityHelpers=new List<PCGID>();
        public static IList<PCGID> IterateAdjacentCorners(this PolyVertex _vertex,byte _height)
        {
            kIdentityHelpers.Clear();
            var srcID = new PCGID(_vertex.m_Identity, _height);
            if (srcID.TryDownward(out var lowerID))
                kIdentityHelpers.Add(lowerID);
            foreach (var vertex in _vertex.IterateNearbyVertices())
                kIdentityHelpers.Add(new PCGID(vertex.m_Identity,_height));
            if (srcID.TryUpward(out var upperID))
                kIdentityHelpers.Add(upperID);
            return kIdentityHelpers;
        }
        public static IList<PCGID> IterateNearbyCorners(this PolyVertex _vertex, byte _height)
        {
            kIdentityHelpers.Clear();
            var nearbyVertices = _vertex.IterateNearbyVertices();
            var nearbyVertexCount = nearbyVertices.Count;
            for(int i=0;i<nearbyVertexCount;i++)
                kIdentityHelpers.Add(new PCGID(nearbyVertices[i].m_Identity,_height));
            return kIdentityHelpers;
        }
        public static IList<PCGID> IterateIntervalCorners(this PolyVertex _vertex, byte _height)
        {
            kIdentityHelpers.Clear();
            var intervalVertices = _vertex.IterateIntervalVertices();
            var intervalVerticesCount = intervalVertices.Count;
            for(int i=0;i<intervalVerticesCount;i++)
                kIdentityHelpers.Add(new PCGID(intervalVertices[i].m_Identity,_height));
            return kIdentityHelpers;
        }
        
        public static IList<PCGID> IterateRelativeVoxels(this PolyVertex _vertex,byte _height)
        {
            kIdentityHelpers.Clear();
            var nextHeight = UByte.ForwardOne(_height);
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                kIdentityHelpers.Add(new PCGID(quadID, _height));
                kIdentityHelpers.Add(new PCGID(quadID, nextHeight));
            }
            return kIdentityHelpers;
        }
    }
}
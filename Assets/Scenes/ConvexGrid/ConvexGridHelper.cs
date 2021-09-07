using System;
using System.Collections;
using System.Collections.Generic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Tile;
using UnityEngine;

namespace ConvexGrid
{

    public static class ConvexGridHelper
    {
        public static readonly Vector3 m_TileHeight = 4f*Vector3.up;
        public static readonly Vector3 m_CornerOffset = m_TileHeight / 2f;
        
        public static int m_SmoothTimes=256;
        public static float m_SmoothFactor = 0.4f;
        static Matrix4x4 TransformMatrix=Matrix4x4.identity;
        static Matrix4x4 InvTransformMatrix=Matrix4x4.identity;
        public static void InitMatrix(Transform _transform, float _scale)
        {
            TransformMatrix = _transform.localToWorldMatrix*Matrix4x4.Scale(_scale*Vector3.one);
            InvTransformMatrix = _transform.worldToLocalMatrix * Matrix4x4.Scale( Vector3.one / _scale );
        }
        public static void InitRelax(int _smoothTimes,float _smoothFactor)
        {
            m_SmoothTimes = _smoothTimes;
            m_SmoothFactor = _smoothFactor;
        }
        public static Vector3 ToPosition(this Coord _pixel)
        {
            return TransformMatrix* new Vector3(_pixel.x,0,_pixel.y);
        }

        public static Coord ToCoord(this Vector3 _world)
        {
            var coord = InvTransformMatrix * _world;
            return new Coord(coord.x,  coord.z);
        }
        
        public static Vector3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToPixel().ToPosition();
        }

        public static Vector3 GetVoxelHeight(GridPile _id)
        {
            return m_CornerOffset + m_TileHeight * _id.height;
        }

        public static Vector3 GetCornerHeight(GridPile _id)
        {
            return m_TileHeight * _id.height;
        }
    }
}
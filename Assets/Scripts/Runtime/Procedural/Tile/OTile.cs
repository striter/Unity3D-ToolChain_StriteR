using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Tile
{
    [Flags]
    public enum ETileDirection
    {
        Invalid = -1,
        Forward = 1,
        Right = 2,
        Back = 4,
        Left = 8,

        ForwardRight = 64,
        BackRight = 128,
        BackLeft = 256,
        TopLeft = 512,
    }
    public interface ITile
    {
        TileCoord m_Axis { get; }
    }
    
    [Serializable]
    public struct TileCoord
    {
        public int x;
        public int y;
        public TileCoord(int _axisX, int _axisY)
        {
            x = _axisX;
            y = _axisY;
        }

        public static readonly (ETileDirection,TileCoord)[] kNearbyTiles = {
            (ETileDirection.Left, new TileCoord(-1, 0)),
            (ETileDirection.Right, new TileCoord(1, 0)),
            (ETileDirection.Forward, new TileCoord(0, 1)),
            (ETileDirection.Back ,new TileCoord(0, -1))};
    
        public TileCoord[] GetNearbyTiles()
        {
            var tile = this; 
            return kNearbyTiles.Select(p => p.Item2+tile).ToArray();
        }

        public (ETileDirection, TileCoord)[] GetNearbyTilesDirection()
        {
            var tile = this; 
            return kNearbyTiles.Select(p =>(p.Item1, p.Item2+tile)).ToArray();
        }

        public static TileCoord operator -(TileCoord a) => new TileCoord(-a.x, -a.y);
        public static bool operator ==(TileCoord a, TileCoord b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(TileCoord a, TileCoord b) => a.x != b.x || a.y != b.y;
        public static TileCoord operator -(TileCoord a, TileCoord b) => new TileCoord(a.x - b.x, a.y - b.y);
        public static TileCoord operator +(TileCoord a, TileCoord b) => new TileCoord(a.x + b.x, a.y + b.y);
        public static TileCoord operator *(TileCoord a, TileCoord b) => new TileCoord(a.x * b.x, a.y * b.y);
        public static TileCoord operator /(TileCoord a, TileCoord b) => new TileCoord(a.x / b.x, a.y / b.y);

        public static TileCoord operator *(TileCoord a, int b) => new TileCoord(a.x * b, a.y * b);
        public static TileCoord operator /(TileCoord a, int b) => new TileCoord(a.x / b, a.y / b);
        public TileCoord Inverse() => new TileCoord(y, x);
        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => x + "," + y;
        public int sqrMagnitude => x * x + y * y;
        
        public static readonly TileCoord kZero = new TileCoord(0, 0);
        public static readonly TileCoord kOne = new TileCoord(1, 1);
        public static readonly TileCoord kNegOne = new TileCoord(-1, -1);
        public static readonly TileCoord kBack = new TileCoord(0, -1);
        public static readonly TileCoord kRight = new TileCoord(1, 0);
        public static readonly TileCoord kLeft = new TileCoord(-1, 0);
        public static readonly TileCoord kForward = new TileCoord(0, 1);
    }
    
    public struct TileBounds
    {
        public TileCoord m_Origin { get; private set; }
        public TileCoord m_Size { get; private set; }
        public TileCoord m_End { get; private set; }
        public bool Contains(TileCoord axis) => axis.x >= m_Origin.x && axis.x <= m_End.x && axis.y >= m_Origin.y && axis.y <= m_End.y;
        public override string ToString() => m_Origin.ToString() + "|" + m_Size.ToString();
        public bool Intersects(TileBounds targetBounds)
        {
            TileCoord[] sourceAxies = new TileCoord[] { m_Origin, m_End, m_Origin + new TileCoord(m_Size.x, 0), m_Origin + new TileCoord(0, m_Size.y) };
            for (int i = 0; i < sourceAxies.Length; i++)
                if (targetBounds.Contains(sourceAxies[i]))
                    return true;
            TileCoord[] targetAxies = new TileCoord[] { targetBounds.m_Origin, targetBounds.m_End, targetBounds.m_Origin + new TileCoord(targetBounds.m_Size.x, 0), targetBounds.m_Origin + new TileCoord(0, targetBounds.m_Size.y) };
            for (int i = 0; i < targetAxies.Length; i++)
                if (Contains(targetAxies[i]))
                    return true;
            return false;
        }

        public TileBounds(TileCoord origin, TileCoord size)
        {
            m_Origin = origin;
            m_Size = size;
            m_End = m_Origin + m_Size;
        }
    }
}
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
        Int2 m_Axis { get; }
    }
    
    public struct TileBounds
    {
        public Int2 m_Origin { get; private set; }
        public Int2 m_Size { get; private set; }
        public Int2 m_End { get; private set; }
        public bool Contains(Int2 axis) => axis.x >= m_Origin.x && axis.x <= m_End.x && axis.y >= m_Origin.y && axis.y <= m_End.y;
        public override string ToString() => m_Origin.ToString() + "|" + m_Size.ToString();
        public bool Intersects(TileBounds targetBounds)
        {
            Int2[] sourceAxies = new Int2[] { m_Origin, m_End, m_Origin + new Int2(m_Size.x, 0), m_Origin + new Int2(0, m_Size.y) };
            for (int i = 0; i < sourceAxies.Length; i++)
                if (targetBounds.Contains(sourceAxies[i]))
                    return true;
            Int2[] targetAxies = new Int2[] { targetBounds.m_Origin, targetBounds.m_End, targetBounds.m_Origin + new Int2(targetBounds.m_Size.x, 0), targetBounds.m_Origin + new Int2(0, targetBounds.m_Size.y) };
            for (int i = 0; i < targetAxies.Length; i++)
                if (Contains(targetAxies[i]))
                    return true;
            return false;
        }

        public TileBounds(Int2 origin, Int2 size)
        {
            m_Origin = origin;
            m_Size = size;
            m_End = m_Origin + m_Size;
        }
    }
}
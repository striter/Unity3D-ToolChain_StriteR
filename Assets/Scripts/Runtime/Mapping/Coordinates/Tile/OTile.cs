using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
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
    
    public class TileGraph:IGraph<int2>,IGraphMapping<int2>
    {
        private float m_Size;
        public TileGraph(float _size)
        {
            m_Size = _size;
        }

        public IEnumerable<int2> GetAdjacentNodes(int2 _src)
        {
            yield return new int2(_src.x - 1, _src.y);
            yield return new int2(_src.x, _src.y - 1);
            yield return new int2(_src.x + 1, _src.y);
            yield return new int2(_src.x, _src.y + 1);
            yield return new int2(_src.x + 1, _src.y - 1);
            yield return new int2(_src.x + 1, _src.y + 1);
            yield return new int2(_src.x - 1, _src.y + 1);
            yield return new int2(_src.x - 1, _src.y - 1);
        }

        public bool PositionToNode(float3 _position, out int2 _node)
        {
            _node = new int2((int)math.floor(_position.x / m_Size), (int)math.floor(_position.z / m_Size));
            return true;
        }

        public bool NodeToPosition(int2 _node, out float3 _position)
        {
            _position = new Vector3(_node.x * m_Size, 0, _node.y * m_Size);
            return true;
        }

        public GBox NodeToBoundingBox(int2 _node) => GBox.Minmax(new float3(_node.x,0f,_node.y)*m_Size,new float3(_node.x+1,0f,_node.y+1)*m_Size);
    }
}
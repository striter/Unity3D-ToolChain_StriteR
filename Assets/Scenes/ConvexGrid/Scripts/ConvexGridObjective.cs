using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
#region Grid
    public class ConvexVertex
    {
        public HexCoord m_Hex;
        public Coord m_Coord;
        public readonly List<ConvexQuad> m_NearbyQuads = new List<ConvexQuad>();
        private readonly int[] m_NearbyQuadsStartIndex = new int[6];
        private static readonly int[] s_QuadIndexHelper = new int[4];
        public int[] GetQuadVertsCW(int _index)
        {
            int offset = m_NearbyQuadsStartIndex[_index];
            s_QuadIndexHelper[0] = offset;
            s_QuadIndexHelper[1] = (offset + 1) % 4;
            s_QuadIndexHelper[2] = (offset + 2) % 4;
            s_QuadIndexHelper[3] = (offset + 3) % 4;
            return s_QuadIndexHelper;
        }
        public void AddNearbyQuads(ConvexQuad _quad)
        {
            m_NearbyQuadsStartIndex[m_NearbyQuads.Count] = _quad.m_HexQuad.FindIndex(p => p == m_Hex);
            m_NearbyQuads.Add(_quad);
        }
    }
    public class ConvexQuad
    {
        public HexCoord m_Identity => m_HexQuad.identity;
        public HexQuad m_HexQuad { get; private set; }
        public Quad<Coord> m_CoordQuad { get; private set; }
        public Coord m_CoordCenter { get; private set; }
        public readonly ConvexVertex[] m_Vertices = new ConvexVertex[4];
        public ConvexQuad(HexQuad _hexQuad,Dictionary<HexCoord,ConvexVertex> _vertices)
        {
            m_HexQuad = _hexQuad;
            m_CoordQuad=new Quad<Coord>(
                _vertices[m_HexQuad[0]].m_Coord,
                _vertices[m_HexQuad[1]].m_Coord,
                _vertices[m_HexQuad[2]].m_Coord,
                _vertices[m_HexQuad[3]].m_Coord);
            m_Vertices[0] = _vertices[m_HexQuad[0]];
            m_Vertices[1] = _vertices[m_HexQuad[1]];
            m_Vertices[2] = _vertices[m_HexQuad[2]];
            m_Vertices[3] = _vertices[m_HexQuad[3]];
            m_CoordCenter = m_CoordQuad.GetBaryCenter();
        }
    }
    public class ConvexArea
    {
        public HexagonArea m_Identity;
        public readonly List<ConvexQuad> m_Quads = new List<ConvexQuad>();
        public readonly List<ConvexVertex> m_Vertices = new List<ConvexVertex>();
        public ConvexArea(HexagonArea identity)
        {
            m_Identity = identity;
        }
    }
    #endregion
    
#region Pile
    [Serializable]
    public struct PileID:IEquatable<PileID>
    {
        public HexCoord location;
        public byte height;

        public PileID(HexCoord _location, byte _height)
        {
            location = _location;
            height = _height;
        }

        public override string ToString() => $"{location}|{height}";

        public bool Equals(PileID other)=> location.Equals(other.location) && height == other.height;

        public override bool Equals(object obj)=> obj is PileID other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (location.GetHashCode() * 397) ^ height.GetHashCode();
            }
        }
    }

    public enum EPileStatus
    {
        Top,
        Medium,
        Bottom,
    }
    public interface IPile
    {
        EPileStatus Status { get; set; }
    }
    public class PilePool<T> : IEnumerable<T> where T:PoolBehaviour<PileID>,IPile
    {
        private readonly Dictionary<HexCoord, List<byte>> m_Piles = new Dictionary<HexCoord, List<byte>>();
        readonly TObjectPoolMono<PileID,T> m_Pool;

        public PilePool(Transform _transform)
        {
            m_Pool = new TObjectPoolMono<PileID, T>(_transform);
        }
        public bool Contains(PileID _pileID)
        {
            if (!m_Piles.ContainsKey(_pileID.location))
                return false;
            return m_Piles[_pileID.location].Contains(_pileID.height);
        }

        public bool Contains(HexCoord _coord) => m_Piles.ContainsKey(_coord);
        public T Get(PileID _pileID)=> m_Pool.Get(_pileID);
        public T Spawn(PileID _pileID)
        {
            T item = m_Pool.Spawn( _pileID);
            var location = _pileID.location;
            if (!m_Piles.ContainsKey(location))
                m_Piles.Add(location,TSPoolList<byte>.Spawn());
            m_Piles[location].Add(_pileID.height);
            RefreshPileStatus(location);
            return item;
        }
        public T Recycle(PileID _pileID)
        {
            T item = m_Pool.Recycle(_pileID);
            var location = _pileID.location;
            m_Piles[location].Remove(_pileID.height);
            RefreshPileStatus(_pileID.location);
            if (m_Piles[location].Count == 0)
            {
                TSPoolList<byte>.Recycle(m_Piles[location]);
                m_Piles.Remove(location);
            }
            return item;
        }

        void RefreshPileStatus(HexCoord _location)
        {
            var count = m_Piles[_location].Count;
            for (int i = 0; i < count; i++)
            {
                var id = new PileID(_location,m_Piles[_location][i]);
                var status = EPileStatus.Medium;
                if (i == 0&&id.height==0)
                    status = EPileStatus.Bottom;
                else if (i == count - 1)
                    status = EPileStatus.Top;
                m_Pool.Get(id).Status = status;
            }
        }

        public byte Count(HexCoord _location)
        {
            if (!m_Piles.ContainsKey(_location))
                return 0;
            return (byte)m_Piles[_location].Count;
        }

        public byte Max(HexCoord _location)
        {
            if (!m_Piles.ContainsKey(_location))
                return 0;
            return m_Piles[_location].Max();
        }
        
        public void Clear()
        {
            foreach (var vertex in m_Piles.Keys)
                TSPoolList<byte>.Recycle(m_Piles[vertex]);
            m_Piles.Clear();
            m_Pool.Clear();
        }
        public IEnumerator<T> GetEnumerator() => m_Pool.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }
#endregion

#region Bridge
    public interface IVoxel
    { 
        Transform Transform { get; }
        PileID Identity { get; }
        Qube<PileID> QubeCorners { get; }
        Qube<bool> CornerRelations { get; }
        CubeFacing<bool> SideRelations { get;}
        Quad<Vector2>[] CornerShapeLS { get; }
        IPile Pile { get; }
    }

    public interface ICorner
    {
        Transform Transform { get; }
        PileID Identity { get; }
    }
#endregion
}
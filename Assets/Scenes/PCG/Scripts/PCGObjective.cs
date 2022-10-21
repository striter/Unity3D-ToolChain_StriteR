using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Procedural;
using Procedural.Hexagon.Area;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PCG
{
    #region Runtime
    public class PCGDefines<T> where T : struct,IEquatable<T>
    {
        [Serializable]
        public struct SurfaceID:IEquatable<SurfaceID>,IEqualityComparer<SurfaceID>
        {
            public T value;

            public SurfaceID(T _value)
            {
                value = _value;
            }

            public static implicit operator SurfaceID(T _value) => new SurfaceID(_value);
            public bool Equals(SurfaceID other) => value.Equals(other.value);

            public bool Equals(SurfaceID x, SurfaceID y) => x.value.Equals(y.value);
            public static bool operator ==(SurfaceID _src, SurfaceID _dst) => _src.value.Equals(_dst.value);
            public static bool operator !=(SurfaceID _src, SurfaceID _dst) => !_src.value.Equals(_dst.value);

            public override bool Equals(object obj)=> obj is SurfaceID other && Equals(other);
            public override int GetHashCode()=> value.GetHashCode();

            public int GetHashCode(SurfaceID obj)
            {
                unchecked
                {
                    return (value.GetHashCode() * 397);
                }
            }

            public override string ToString() => value.ToString();
        }
        
        public class PolyGridOutput
        {
            public readonly List<PCGID> rValidateCorners = new List<PCGID>();
            public bool rQuadsAvailable;
            public readonly Dictionary<Int2, SurfaceID> rValidateQuads = new Dictionary<Int2, SurfaceID>();
        }

        public class PolyArea
        {
            public readonly HexagonArea m_Identity;
            public readonly List<PolyQuad> m_Quads = new List<PolyQuad>();
            public readonly List<PolyVertex> m_Vertices = new List<PolyVertex>();
            public PolyArea(HexagonArea identity)
            {
                m_Identity = identity;
            }
        }

        public class PolyChunk
        {
            public SurfaceID m_Identity;
            public Coord m_Center;
            public readonly List<int> m_QuadIDs=new List<int>();
        }

        public class PolyVertex
        {
            public SurfaceID m_Identity;
            public Coord m_Coord;
            public bool m_Invalid;

            public readonly List<PolyQuad> m_NearbyQuads = new List<PolyQuad>();
            public readonly List<SurfaceID> m_NearbyVertIds = new List<SurfaceID>();
            private readonly int[] m_QuadIndexOffsets = new int[6];

            public SurfaceID m_ForwardVertex;
            public SurfaceID m_RightVertex;
            public void Initialize(Dictionary<SurfaceID, PolyVertex> _vertices)
            {
                TSPoolList<SurfaceID>.Spawn(out var allNearbyVertex);
                TSPoolList<(int index, float rad)>.Spawn(out var radHelper);

                for (int i = 0; i < m_NearbyQuads.Count; i++)
                {
                    var quad = m_NearbyQuads[i];
                    var vertStartIndex = quad.m_Hex.IterateFindIndex(p => p .Equals(m_Identity));
                    m_QuadIndexOffsets[i] = vertStartIndex;
                    allNearbyVertex.Add(quad.m_Hex[(vertStartIndex + 1) % 4]);
                }

                for (int i = 0; i < allNearbyVertex.Count; i++)
                    radHelper.Add((i, UMath.GetRadClockWise(Vector2.up, _vertices[allNearbyVertex[i]].m_Coord - m_Coord)));
                radHelper.Sort((a, b) => a.rad > b.rad ? 1 : -1);
                radHelper.Reindex(radHelper.FindLastIndex((a, b) => Mathf.Abs(a.rad) > Mathf.Abs(b.rad) ? 1 : -1));

                var sortIndex = radHelper.Select(p => p.index);
                m_NearbyQuads.SortIndex(sortIndex);
                m_QuadIndexOffsets.SortIndex(radHelper.Select(p => p.index));
                if (!m_Invalid)
                {
                    m_ForwardVertex = allNearbyVertex[radHelper[0].index];
                    m_RightVertex = allNearbyVertex[radHelper[1].index];
                }

                TSPoolList<(int index, float rad)>.Recycle(radHelper);
                TSPoolList<SurfaceID>.Recycle(allNearbyVertex);
            }

            public void AddNearbyQuads(PolyQuad _quad)
            {
                m_NearbyQuads.Add(_quad);
                foreach (var vert in _quad.m_Vertices)
                {
                    if (!vert.m_Identity.Equals(m_Identity))
                    {
                        m_NearbyVertIds.TryAdd(vert.m_Identity);
                    }
                }
            }

            private static readonly int[] s_QuadIndexHelper = new int[4];
            public int[] GetQuadVertsArrayCW(int _index)
            {
                int offset = m_QuadIndexOffsets[_index];
                s_QuadIndexHelper[0] = offset;
                s_QuadIndexHelper[1] = (offset + 1) % 4;
                s_QuadIndexHelper[2] = (offset + 2) % 4;
                s_QuadIndexHelper[3] = (offset + 3) % 4;
                return s_QuadIndexHelper;
            }

            int GetQuadVertsCW(int _index, int _vertIndex) => (m_QuadIndexOffsets[_index] + _vertIndex) % 4;

            private static readonly List<PolyVertex> kVerticesHelper = new List<PolyVertex>();
            public IList<PolyVertex> IterateNearbyVertices()
            {
                kVerticesHelper.Clear();
                var count = m_NearbyQuads.Count;
                for (int i = 0; i < count; i++)
                    kVerticesHelper.Add(m_NearbyQuads[i].m_Vertices[GetQuadVertsCW(i, 1)]);
                return kVerticesHelper;
            }

            public IList<PolyVertex> IterateIntervalVertices()
            {
                kVerticesHelper.Clear();
                var count = m_NearbyQuads.Count;
                for (int i = 0; i < count; i++)
                    kVerticesHelper.Add(m_NearbyQuads[i].m_Vertices[GetQuadVertsCW(i, 2)]);
                return kVerticesHelper;
            }
        }

        public class PolyQuad
        {
            public SurfaceID m_Identity { get; }
            public Quad<SurfaceID> m_Hex { get; }
            public Quad<PolyVertex> m_Vertices { get; }
            public Quad<Coord> m_CoordWS { get; }

            public Coord m_CenterWS { get; }
            public float m_Orientation { get; }

            public PolyQuad(SurfaceID _identity, Quad<SurfaceID> _hexQuad, Dictionary<SurfaceID, PolyVertex> _vertices)
            {
                m_Identity = _identity;
                TSPoolList<(int index, float rad)>.Spawn(out var radHelper);

                var srcCoords = _hexQuad.Convert(p => _vertices[p].m_Coord);
                m_CenterWS = (srcCoords.B+srcCoords.L+srcCoords.F+srcCoords.R)/4;
                for (int i = 0; i < 4; i++)
                    radHelper.Add((i, UMath.GetRadClockWise(Vector2.up, srcCoords[i] - m_CenterWS)));
                radHelper.Sort((a, b) => a.rad > b.rad ? 1 : -1);

                m_Orientation = radHelper[0].rad * UMath.kDeg2Rad;

                m_Hex = new Quad<SurfaceID>(_hexQuad[radHelper[0].index], _hexQuad[radHelper[1].index], _hexQuad[radHelper[2].index], _hexQuad[radHelper[3].index]);
                m_Vertices = m_Hex.Convert(p => _vertices[p]);
                m_CoordWS = m_Vertices.Convert(p => p.m_Coord);

                TSPoolList<(int index, float rad)>.Recycle(radHelper);
            }

            public Qube<PCGID> Corners(byte _srcHeight)
            {
                var corners = new Qube<PCGID>();
                for (int i = 0; i < 8; i++)
                {
                    var corner = m_Hex[i % 4];
                    byte height = i >= 4 ? _srcHeight : UByte.BackOne(_srcHeight);
                    corners[i]= new PCGID(corner,height);
                }
                return corners;
            }
        }

        #region Pile
        [Serializable]
        public struct PCGID : IEquatable<PCGID>, IEqualityComparer<PCGID>
        {
            public SurfaceID location;
            public byte height;

            public PCGID(SurfaceID _location, byte _height)
            {
                location = _location;
                height = _height;
            }

            public override string ToString() => $"{location}|{height}";

            public bool Equals(PCGID other) => location.Equals(other.location) && height == other.height;

            public override bool Equals(object obj) => obj is PCGID other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (location.GetHashCode() * 397) ^ height.GetHashCode();
                }
            }

            public bool Equals(PCGID x, PCGID y)
            {
                return x.location.Equals(y.location) && x.height == y.height;
            }

            public int GetHashCode(PCGID obj)
            {
                unchecked
                {
                    return (obj.location.GetHashCode() * 397) ^ obj.height.GetHashCode();
                }
            }

            public static bool operator ==(PCGID _src, PCGID _dst) => _src.Equals(_dst);

            public static bool operator !=(PCGID _src, PCGID _dst) => !_src.Equals(_dst);
        }

        public class PilePool<Y> : IEnumerable<Y> where Y : PoolBehaviour<PCGID>
        {
            private readonly Dictionary<SurfaceID, List<byte>> m_Piles = new Dictionary<SurfaceID, List<byte>>();
            readonly TObjectPoolMono<PCGID, Y> m_Pool;

            public PilePool(Transform _transform)
            {
                m_Pool = new TObjectPoolMono<PCGID, Y>(_transform);
            }
            public bool Contains(PCGID _pcgid)
            {
                if (!m_Piles.ContainsKey(_pcgid.location))
                    return false;
                return m_Piles[_pcgid.location].Contains(_pcgid.height);
            }

            public bool Contains(SurfaceID _coord) => m_Piles.ContainsKey(_coord);
            public Y this[PCGID _pcgid] => m_Pool.Get(_pcgid);
            public Y Spawn(PCGID _pcgid)
            {
                Y item = m_Pool.Spawn(_pcgid);
                var location = _pcgid.location;
                if (!m_Piles.ContainsKey(location))
                    m_Piles.Add(location, TSPoolList<byte>.Spawn());
                m_Piles[location].Add(_pcgid.height);
                return item;
            }
            public Y Recycle(PCGID _pcgid)
            {
                Y item = m_Pool.Recycle(_pcgid);
                var location = _pcgid.location;
                m_Piles[location].Remove(_pcgid.height);
                if (m_Piles[location].Count == 0)
                {
                    TSPoolList<byte>.Recycle(m_Piles[location]);
                    m_Piles.Remove(location);
                }
                
                return item;
            }

            public byte Count(SurfaceID _location) => (byte)m_Piles[_location].Count;
            public byte Max(SurfaceID _location) => m_Piles[_location].Max();

            public void Clear()
            {
                foreach (var vertex in m_Piles.Keys)
                    TSPoolList<byte>.Recycle(m_Piles[vertex]);
                m_Piles.Clear();
                m_Pool.Clear();
            }
            public IEnumerator<Y> GetEnumerator() => m_Pool.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    #endregion
    }
    
    #endregion

}

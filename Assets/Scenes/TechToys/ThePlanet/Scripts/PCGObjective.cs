using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Geometry;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PCG
{
    #region Runtime
    [Serializable]
    public struct GridID:IEquatable<GridID>,IEqualityComparer<GridID>
    {
        public int value;

        public GridID(int _value)
        {
            value = _value;
        }

        public static implicit operator GridID(int _value) => new GridID(_value);
        public bool Equals(GridID other) => value.Equals(other.value);

        public bool Equals(GridID x, GridID y) => x.value.Equals(y.value);
        public static bool operator ==(GridID _src, GridID _dst) => _src.value.Equals(_dst.value);
        public static bool operator !=(GridID _src, GridID _dst) => !_src.value.Equals(_dst.value);

        public override bool Equals(object obj)=> obj is GridID other && Equals(other);
        public override int GetHashCode()=> value.GetHashCode();

        public int GetHashCode(GridID obj)
        {
            unchecked
            {
                return (value.GetHashCode() * 397);
            }
        }

        public override string ToString() => value.ToString();
    }
    
    public class PCGChunk
    {
        public readonly int m_Identity;
        public readonly List<PCGQuad> m_Quads = new List<PCGQuad>();
        public readonly List<PCGVertex> m_Vertices = new List<PCGVertex>();
        public PCGChunk(int identity)
        {
            m_Identity = identity;
        }
    }

    public class PCGVertex
    {
        public GridID m_Identity;
        public Vector3 m_Position;
        public Vector3 m_Normal;
        public bool m_Invalid;

        public readonly List<PCGQuad> m_NearbyQuads = new List<PCGQuad>();
        public readonly List<GridID> m_NearbyVertIds = new List<GridID>();
        private readonly int[] m_QuadIndexOffsets = new int[6];

        public GridID m_ForwardVertex;
        public GridID m_RightVertex;
        
        public void PreInitialize(PCGQuad _quad)
        {
            m_NearbyQuads.Add(_quad);
            foreach (var vert in _quad.m_Vertices)
                if (!vert.m_Identity.Equals(m_Identity))
                    m_NearbyVertIds.TryAdd(vert.m_Identity);
        }
        public void Initialize(Dictionary<GridID, PCGVertex> _vertices)
        {
            TSPoolList<GridID>.Spawn(out var allNearbyVertex);
            TSPoolList<(int index, float rad)>.Spawn(out var radHelper);

            for (int i = 0; i < m_NearbyQuads.Count; i++)
            {
                var quad = m_NearbyQuads[i];
                var vertStartIndex = quad.m_Indexes.IterateFindIndex(p => p .Equals(m_Identity));
                m_QuadIndexOffsets[i] = vertStartIndex;
                allNearbyVertex.Add(quad.m_Indexes[(vertStartIndex + 1) % 4]);
            }

            //Reorder
            Vector3 forward = _vertices[allNearbyVertex[0]].m_Position - m_Position;
            for (int i = 0; i < allNearbyVertex.Count; i++)
                radHelper.Add((i,  Vector3.Angle(forward, _vertices[allNearbyVertex[i]].m_Position - m_Position)));
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
            TSPoolList<GridID>.Recycle(allNearbyVertex);
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

        private static readonly List<PCGVertex> kVerticesHelper = new List<PCGVertex>();
        public IList<PCGVertex> IterateNearbyVertices()
        {
            kVerticesHelper.Clear();
            var count = m_NearbyQuads.Count;
            for (int i = 0; i < count; i++)
                kVerticesHelper.Add(m_NearbyQuads[i].m_Vertices[GetQuadVertsCW(i, 1)]);
            return kVerticesHelper;
        }

        public IList<PCGVertex> IterateIntervalVertices()
        {
            kVerticesHelper.Clear();
            var count = m_NearbyQuads.Count;
            for (int i = 0; i < count; i++)
                kVerticesHelper.Add(m_NearbyQuads[i].m_Vertices[GetQuadVertsCW(i, 2)]);
            return kVerticesHelper;
        }
    }

    public class PCGQuad
    {
        public GridID m_Identity { get; }

        public Quad<PCGVertex> m_Vertices { get; }
        public Quad<GridID> m_Indexes { get; }
        public Vector3 position { get; }
        public Quaternion rotation { get; private set; }
        public Vector3 forward { get; }
        
        public TrapezoidQuad m_ShapeOS { get; private set; }
        public TrapezoidQuad m_ShapeWS { get; private set; }

        public PCGQuad(GridID _identity, PQuad _hexQuad, Dictionary<GridID, PCGVertex> _vertices)
        {
            m_Identity = _identity;
            m_Indexes = _hexQuad.Convert(p=>new GridID(p));
            m_Vertices = m_Indexes.Convert(p => _vertices[p]);

            m_ShapeWS = new TrapezoidQuad(m_Vertices.Convert(p=>p.m_Position),m_Vertices.Convert(p=>p.m_Normal));
            
            var shapeWS = new GQuad( m_Vertices.Convert(p => p.m_Position));
            position = shapeWS.GetBaryCenter();
            forward = m_Vertices.R.m_Position - m_Vertices.B.m_Position;
            rotation = Quaternion.LookRotation(forward,m_ShapeWS.normal);
            
            var invRotation = Quaternion.Inverse(rotation);
            m_ShapeOS = new TrapezoidQuad(m_Vertices.Convert(p=>invRotation*(p.m_Position-position)),m_Vertices.Convert(p=>invRotation*p.m_Normal));
        }

        public Qube<PCGID> Corners(byte _srcHeight)
        {
            var corners = new Qube<PCGID>();
            for (int i = 0; i < 8; i++)
            {
                var corner = m_Indexes[i % 4];
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
        public GridID location;
        public byte height;
        
        public PCGID(GridID _location, byte _height)
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
        public int GetIdentity(int _typeID) => location.value * 10000 + height * 10 + _typeID;
    }

    public struct TrapezoidQuad
    {
        public Quad<Vector3> positions;
        public Quad<Vector3> normals;
        public Vector3 normal;
        public TrapezoidQuad(Quad<Vector3> _positions,Quad<Vector3> _normals,float _normalOffset = 0f,Vector3 _baseOffset = default)
        {
            normals = _normals;
            normal = _normals.Average().normalized;
            positions = default;
            for(int i=0;i<4;i++)
                positions[i] = _positions[i]+_normals[i]*_normalOffset-_baseOffset;
        }

        public Vector3 GetPoint(float _u,float _v) => umath.bilinearLerp(positions.B,positions.L,positions.F,positions.R,_u,_v);
        public Vector3 GetNormal(float _u, float _v)=> umath.bilinearLerp(normals.B,normals.L,normals.F,normals.R,_u,_v).normalized;
        public Vector3 GetPoint(float _u, float _v, float _w) => GetPoint(_u, _v) + GetNormal(_u,_v)*_w;

        public void DrawGizmos()
        {
            Gizmos_Extend.DrawLinesConcat(positions.Iterate());
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(positions[i],positions[i]+normals[i]);
        }
        
    }

    public class PilePool<Y> : IEnumerable<Y> where Y : PoolBehaviour<PCGID>
    {
        private readonly Dictionary<GridID, List<byte>> m_Piles = new Dictionary<GridID, List<byte>>();
        readonly ObjectPoolMono<PCGID, Y> m_Pool;

        public PilePool(Transform _transform)
        {
            m_Pool = new ObjectPoolMono<PCGID, Y>(_transform);
        }
        public bool Contains(PCGID _pcgid)
        {
            if (!m_Piles.ContainsKey(_pcgid.location))
                return false;
            return m_Piles[_pcgid.location].Contains(_pcgid.height);
        }

        public bool Contains(GridID _coord) => m_Piles.ContainsKey(_coord);
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

        public byte Count(GridID _location) => (byte)m_Piles[_location].Count;
        public byte Max(GridID _location) => m_Piles[_location].Max();

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
    
    #endregion

}

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

namespace PolyGrid
{
    
#region Bridge
    public interface IPolyGridControl
    {
        void Init(Transform _transform);
        void Tick(float _deltaTime);
        void Clear();
    }

    public interface IVoxel
    { 
        PolyID Identity { get; }
        Transform Transform { get; }
        PolyQuad Quad { get; }
        Qube<PolyID> QubeCorners { get; }
        Qube<bool> CornerRelations { get; }
        CubeFacing<bool> SideRelations { get;}
        Quad<Vector2>[] CornerShapeLS { get; }
    }

    public interface ICorner
    {
        PolyID Identity { get; }
        Transform Transform { get; }
        PolyVertex Vertex { get; }
        List<PolyID> NearbyCorners { get; }
        List<PolyID> NearbyVoxels { get; }
    }

    public interface IPolyGridVertexCallback
    {
        void OnPopulateVertex(PolyVertex _vertex);
        void OnDeconstructVertex(HexCoord _vertexID);
    }

    public interface IPolyGridQuadCallback
    {
        void OnPopulateQuad(PolyQuad _quad);
        void OnDeconstructQuad(HexCoord _quadID);
    }
    
    public interface IPolyGridCornerCallback
    {
        void OnPopulateCorner(ICorner _corner);
        void OnDeconstructCorner(PolyID _cornerID);
    }

    public interface IPolyGridVoxelCallback
    {
        void OnPopulateVoxel(IVoxel _voxel);
        void OnDeconstructVoxel(PolyID _voxelID);
    }

    public interface IPolyGridModifyCallback
    {
        void OnVertexModify(PolyVertex _vertex, byte _height, bool _construct);
    }
#endregion
    
#region GridRuntime
    public class PolyVertex
    {
        public HexCoord m_Identity;
        public Coord m_Coord;
        public bool m_Invalid;
        public readonly List<PolyQuad> m_NearbyQuads = new List<PolyQuad>();
        public readonly List<HexCoord> m_NearbyVertex = new List<HexCoord>();
        private readonly int[] m_QuadIndexOffsets = new int[6];
        public void AddNearbyQuads(PolyQuad _quad)
        {
            var quadStartIndex=_quad.m_HexQuad.IterateFindIndex(p => p == m_Identity);
            m_QuadIndexOffsets[m_NearbyQuads.Count] = quadStartIndex;
            m_NearbyVertex.TryAdd(_quad.m_HexQuad[(quadStartIndex+1)%4]);
            m_NearbyVertex.TryAdd(_quad.m_HexQuad[(quadStartIndex+3)%4]);
            m_NearbyQuads.Add(_quad);
        }

        private static readonly int[] s_QuadIndexHelper = new int[4];
        public int[] GetQuadVertsCW(int _index)
        {
            int offset = m_QuadIndexOffsets[_index];
            s_QuadIndexHelper[0] = offset;
            s_QuadIndexHelper[1] = (offset + 1) % 4;
            s_QuadIndexHelper[2] = (offset + 2) % 4;
            s_QuadIndexHelper[3] = (offset + 3) % 4;
            return s_QuadIndexHelper;
        }
    }
    
    public class PolyQuad
    {
        public HexCoord m_Identity => m_HexQuad.identity;
        public HexQuad m_HexQuad { get; private set; }
        public Quad<Coord> m_CoordQuad { get; private set; }
        public Coord m_CoordCenter { get; private set; }
        public readonly PolyVertex[] m_Vertices = new PolyVertex[4];
        public PolyQuad(HexQuad _hexQuad,Dictionary<HexCoord,PolyVertex> _vertices)
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
    #endregion
    
#region Pile
    [Serializable]
    public struct PolyID:IEquatable<PolyID>,IEqualityComparer<PolyID>
    {
        public HexCoord location;
        public byte height;

        public PolyID(HexCoord _location, byte _height)
        {
            location = _location;
            height = _height;
        }

        public override string ToString() => $"{location}|{height}";

        public bool Equals(PolyID other)=> location.Equals(other.location) && height == other.height;

        public override bool Equals(object obj)=> obj is PolyID other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (location.GetHashCode() * 397) ^ height.GetHashCode();
            }
        }

        public bool Equals(PolyID x, PolyID y)
        {
            return x.location.Equals(y.location) && x.height == y.height;
        }

        public int GetHashCode(PolyID obj)
        {
            unchecked
            {
                return (obj.location.GetHashCode() * 397) ^ obj.height.GetHashCode();
            }
        }
    }

    public class PilePool<T> : IEnumerable<T> where T:PoolBehaviour<PolyID>
    {
        private readonly Dictionary<HexCoord, List<byte>> m_Piles = new Dictionary<HexCoord, List<byte>>();
        readonly TObjectPoolMono<PolyID,T> m_Pool;

        public PilePool(Transform _transform)
        {
            m_Pool = new TObjectPoolMono<PolyID, T>(_transform);
        }
        public bool Contains(PolyID _polyID)
        {
            if (!m_Piles.ContainsKey(_polyID.location))
                return false;
            return m_Piles[_polyID.location].Contains(_polyID.height);
        }

        public bool Contains(HexCoord _coord) => m_Piles.ContainsKey(_coord);
        public T this[PolyID _polyID]=> m_Pool.Get(_polyID);
        public T Spawn(PolyID _polyID)
        {
            T item = m_Pool.Spawn( _polyID);
            var location = _polyID.location;
            if (!m_Piles.ContainsKey(location))
                m_Piles.Add(location,TSPoolList<byte>.Spawn());
            m_Piles[location].Add(_polyID.height);
            return item;
        }
        public T Recycle(PolyID _polyID)
        {
            T item = m_Pool.Recycle(_polyID);
            var location = _polyID.location;
            m_Piles[location].Remove(_polyID.height);
            if (m_Piles[location].Count == 0)
            {
                TSPoolList<byte>.Recycle(m_Piles[location]);
                m_Piles.Remove(location);
            }
            return item;
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
}
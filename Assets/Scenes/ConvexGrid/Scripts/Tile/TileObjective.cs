using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural.Hexagon;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    [Serializable]
    public struct PileID:IEquatable<PileID>
    {
        public HexCoord gridID;
        public byte height;

        public PileID(HexCoord _gridID, byte _height)
        {
            gridID = _gridID;
            height = _height;
        }

        public override string ToString() => $"{gridID}|{height}";

        public bool Equals(PileID other)=> gridID.Equals(other.gridID) && height == other.height;

        public override bool Equals(object obj)=> obj is PileID other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (gridID.GetHashCode() * 397) ^ height.GetHashCode();
            }
        }
    }
    public class PilePool<T> : IEnumerable<T> where T:PoolBehaviour<PileID>
    {
        private readonly Dictionary<HexCoord, List<byte>> m_Items = new Dictionary<HexCoord, List<byte>>();
        readonly TObjectPoolMono<PileID,T> m_Pool;

        public PilePool(Transform _transform)
        {
            m_Pool = new TObjectPoolMono<PileID, T>(_transform);
        }
        public bool Contains(PileID _pileID)
        {
            if (!m_Items.ContainsKey(_pileID.gridID))
                return false;
            return m_Items[_pileID.gridID].Contains(_pileID.height);
        }

        public bool Contains(HexCoord _coord) => m_Items.ContainsKey(_coord);
        public T Spawn(PileID _pileID)
        {
            T item = m_Pool.Spawn( _pileID);
            AddVertex(_pileID.gridID);
            m_Items[_pileID.gridID].Add(_pileID.height);
            return item;
        }

        public T Get(PileID _pileID)=> m_Pool.Get(new PileID(_pileID.gridID, _pileID.height));
        public T Recycle(PileID _pileID)
        {
            T item = m_Pool.Recycle(new PileID(_pileID.gridID,_pileID.height));
            m_Items[_pileID.gridID].Remove(_pileID.height);
            RemoveVertex(_pileID.gridID);
            return item;
        }

        public byte Count(HexCoord _location)
        {
            if (!m_Items.ContainsKey(_location))
                return 0;
            return (byte)m_Items[_location].Count;
        }

        public byte Max(HexCoord _location)
        {
            if (!m_Items.ContainsKey(_location))
                return 0;
            return m_Items[_location].Max();
        }
        void AddVertex(HexCoord _vertex)
        {
            if (m_Items.ContainsKey(_vertex))
                return;
            m_Items.Add(_vertex,TSPoolList<byte>.Spawn());
        }

        void RemoveVertex(HexCoord _vertex)
        {
            if (m_Items[_vertex].Count != 0)
                return;
            TSPoolList<byte>.Recycle(m_Items[_vertex]);
            m_Items.Remove(_vertex);
        }


        public void Clear()
        {
            foreach (var vertex in m_Items.Keys)
                TSPoolList<byte>.Recycle(m_Items[vertex]);
            m_Items.Clear();
            m_Pool.Clear();
        }
        public IEnumerator<T> GetEnumerator() => m_Pool.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }

}
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
    public struct BQuad : IQuad<bool>
    {
        public bool vB { get; set; }
        public bool vL { get; set; }
        public bool vF { get; set; }
        public bool vR { get; set; }

        public BQuad(bool _vB, bool _vL, bool _vF, bool _vR)
        {
            vB = _vB;
            vL = _vL;
            vF = _vF;
            vR = _vR;
        }
        public bool this[int _index]=>this.GetVertex<BQuad,bool>(_index); 
        public bool this[EQuadCorner _corner] =>this.GetVertex<BQuad,bool>(_corner);
    }

    public struct BCubeFacing:ICubeFace<bool>
    {
        public bool fBL { get; set; }
        public bool fLF { get; set; }
        public bool fFR { get; set; }
        public bool fRB { get; set; }
        public bool fT { get; set; }
        public bool fD { get; set; }

        public BCubeFacing( bool _fBL, bool _fLF, bool _fFR, bool _fRB,bool _fT,bool _fD)
        {
            fBL = _fBL;
            fLF = _fLF;
            fFR = _fFR;
            fRB = _fRB;
            fT = _fT;
            fD = _fD;
        }

        public bool this[int _index] => this.GetFacing<BCubeFacing,bool>(_index);

        public bool this[ECubeFacing _facing] => this.GetFacing<BCubeFacing, bool>(_facing);
    }
    public struct BQube : IQube<bool>
    {
        public bool vertDB { get; set; }
        public bool vertDL { get; set; }
        public bool vertDF { get; set; }
        public bool vertDR { get; set; }
        public bool vertTB { get; set; }
        public bool vertTL { get; set; }
        public bool vertTF { get; set; }
        public bool vertTR { get; set; }

        public BQube(BQuad _relationDown,BQuad _relationTop)
        {
            vertDB = _relationDown.vB;
            vertDL = _relationDown.vL;
            vertDF = _relationDown.vF;
            vertDR = _relationDown.vR;
            vertTB = _relationTop.vB;
            vertTL = _relationTop.vL;
            vertTF = _relationTop.vF;
            vertTR = _relationTop.vR;
        }
        public bool this[int _index] => this.GetVertex<BQube,bool>(_index);
        public bool this[EQubeCorner _corner] =>  this.GetVertex<BQube,bool>(_corner);
    }
    
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

        public override string ToString() => height.ToString();

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
using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;

namespace Procedural.Hexagon.Geometry
{
    [Serializable]
    public struct HexTriangle: ITriangle<HexCoord>, IEquatable<HexTriangle>,IEqualityComparer<HexCoord>,IIterate<HexCoord>
    {
        public HexCoord vertex0 { get; set; }
        public HexCoord vertex1 { get; set; }
        public HexCoord vertex2 { get; set; }
        public int Length => 3;
        public HexTriangle(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
        }
        public HexCoord GetElement(int _index) => this[_index];
        public HexCoord this[int index]
        {
            get
            {
                switch (index)
                {
                    default: throw new Exception("Invalid Index:" + index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                }
            }
        }
        public bool Equals(HexTriangle other) => vertex0 == other.vertex0 && vertex1 == other.vertex1 && vertex2 == other.vertex2;

        public bool Equals(HexCoord x, HexCoord y) => x.Equals(y);

        public int GetHashCode(HexCoord obj)
        {
            unchecked
            {
                int hashCode = obj.x;
                hashCode = (hashCode * 397) ^ obj.y;
                hashCode = (hashCode * 397) ^ obj.z;
                return hashCode;
            }
        }
    }

    [Serializable]
    public struct HexQuad:IQuad<HexCoord>, IEquatable<HexQuad>,IIterate<HexCoord>
    {
        public HexCoord m_Identity { get; set; }
        public HexCoord vB { get; set; }
        public HexCoord vL { get; set; }
        public HexCoord vF { get; set; }
        public HexCoord vR { get; set; }
        public int Length => 4;

        public HexQuad((HexCoord _vertex0, HexCoord _vertex1, HexCoord _vertex2, HexCoord _vertex3) _tuple) 
            : this(_tuple._vertex0, _tuple._vertex1, _tuple._vertex2, _tuple._vertex3)
        {
        }

        public HexQuad(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2,HexCoord _vertex3)
        {
            vB = _vertex0;
            vL = _vertex1;
            vF = _vertex2;
            vR = _vertex3;
            m_Identity = vB + vL + vF + vR;
        }

        public HexCoord this[int _index]=>this.GetVertex<HexQuad,HexCoord>(_index); 
        public HexCoord this[EQuadCorners _corner] =>this.GetVertex<HexQuad,HexCoord>(_corner); 
        public HexCoord GetElement(int _index) => this[_index];
        public bool Equals(HexQuad other) => vB == other.vB && vL == other.vL && vF == other.vF&&vR==other.vR;
    }

}
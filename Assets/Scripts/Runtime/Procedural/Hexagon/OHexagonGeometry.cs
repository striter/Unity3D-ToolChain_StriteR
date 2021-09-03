using System;
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
        public HexCoord GetElement(int index) => this[index];
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
        public HexCoord vertex0 { get; set; }
        public HexCoord vertex1 { get; set; }
        public HexCoord vertex2 { get; set; }
        public HexCoord vertex3 { get; set; }
        public int Length => 4;

        public HexQuad((HexCoord _vertex0, HexCoord _vertex1, HexCoord _vertex2, HexCoord _vertex3) _tuple) 
            : this(_tuple._vertex0, _tuple._vertex1, _tuple._vertex2, _tuple._vertex3)
        {
        }

        public HexQuad(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2,HexCoord _vertex3)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertex3 = _vertex3;
        }
        public HexCoord this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default:throw new Exception("Invalid Index:" + _index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                    case 3: return vertex3;
                }
            }
        }
        public HexCoord GetElement(int index) => this[index];
        public bool Equals(HexQuad other) => vertex0 == other.vertex0 && vertex1 == other.vertex1 && vertex2 == other.vertex2&&vertex3==other.vertex3;
    }
}
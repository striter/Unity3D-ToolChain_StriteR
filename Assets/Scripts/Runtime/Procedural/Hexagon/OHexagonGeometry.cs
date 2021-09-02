using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    [Serializable]
    public struct HexTriangle: IEquatable<HexTriangle>,IEqualityComparer<HexCoord>,IIterate<HexCoord>
    {
        public readonly HexCoord vertex0;
        public readonly HexCoord vertex1;
        public readonly HexCoord vertex2;
        public int Length { get; set; }
        public HexTriangle(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            Length = 3;
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
    public struct HexQuad:IEquatable<HexQuad>,IIterate<HexCoord>
    {
        public readonly HexCoord vertex0;
        public readonly HexCoord vertex1;
        public readonly HexCoord vertex2;
        public readonly HexCoord vertex3;
        public int Length { get; set; }
        public HexQuad(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2,HexCoord _vertex3)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertex3 = _vertex3;
            Length = 4;
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
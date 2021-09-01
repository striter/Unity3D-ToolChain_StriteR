using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    [Serializable]
    public struct HexTriangle: IEquatable<HexTriangle>,IEqualityComparer<HexagonCoordC>,IIterate<HexagonCoordC>
    {
        public readonly HexagonCoordC vertex0;
        public readonly HexagonCoordC vertex1;
        public readonly HexagonCoordC vertex2;
        public int Length { get; set; }
        public HexTriangle(HexagonCoordC _vertex0,HexagonCoordC _vertex1,HexagonCoordC _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            Length = 3;
        }
        public HexagonCoordC GetElement(int index) => this[index];
        public HexagonCoordC this[int index]
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

        public bool Equals(HexagonCoordC x, HexagonCoordC y) => x.Equals(y);

        public int GetHashCode(HexagonCoordC obj)
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
    public struct HexQuad:IEquatable<HexQuad>,IIterate<HexagonCoordC>
    {
        public readonly HexagonCoordC vertex0;
        public readonly HexagonCoordC vertex1;
        public readonly HexagonCoordC vertex2;
        public readonly HexagonCoordC vertex3;
        public int Length { get; set; }
        public HexQuad(HexagonCoordC _vertex0,HexagonCoordC _vertex1,HexagonCoordC _vertex2,HexagonCoordC _vertex3)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertex3 = _vertex3;
            Length = 4;
        }
        public HexagonCoordC this[int _index]
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
        public HexagonCoordC GetElement(int index) => this[index];
        public bool Equals(HexQuad other) => vertex0 == other.vertex0 && vertex1 == other.vertex1 && vertex2 == other.vertex2&&vertex3==other.vertex3;
    }
}
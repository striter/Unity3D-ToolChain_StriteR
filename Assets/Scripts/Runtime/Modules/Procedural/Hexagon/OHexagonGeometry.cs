using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    [Serializable]
    public struct HexTriangle: ITriangle<HexCoord>, IEquatable<HexTriangle>,IEqualityComparer<HexTriangle>,IIterate<HexCoord>
    {
        public Triangle<HexCoord> triangle;
        public int Length => triangle.Length;
        public HexTriangle(HexCoord _vertex0,HexCoord _vertex1,HexCoord _vertex2)
        {
            triangle = new Triangle<HexCoord>(_vertex0, _vertex1, _vertex2);
        }
        public HexCoord this[int _index] => triangle[_index];
        public bool Equals(HexTriangle other) => triangle.V0==other.V0&&triangle.V1==other.V1&&triangle.V2==other.V2;
        public HexCoord V0 => triangle.v0;
        public HexCoord V1 => triangle.v1;
        public HexCoord V2 => triangle.v2;

        public bool Equals(HexTriangle x, HexTriangle y)
        {
            return x.triangle.Equals(y.triangle);
        }

        public int GetHashCode(HexTriangle obj)
        {
            return obj.triangle.GetHashCode();
        }
    }

    [Serializable]
    public struct HexQuad:IQuad<HexCoord>, IEquatable<HexQuad>,IIterate<HexCoord>,IEnumerable<HexCoord>
    {
        public HexCoord identity;
        public Quad<HexCoord> quad;
        public int Length => quad.Length;
        public HexQuad((HexCoord _vB, HexCoord _vL, HexCoord _vF, HexCoord _vR) _tuple) 
            : this(_tuple._vB, _tuple._vL, _tuple._vF, _tuple._vR)
        {
        }

        public HexQuad(HexCoord _vB, HexCoord _vL, HexCoord _vF, HexCoord _vR) : this(
            new Quad<HexCoord>(_vB, _vL, _vF, _vR))
        {
        }

        public HexQuad(Quad<HexCoord> _hexQuad)
        {
            quad = _hexQuad;
            identity = _hexQuad.vB + _hexQuad.vL + _hexQuad.vF + _hexQuad.vR;
        }

        public HexCoord this[int _index]
        {
            get => quad[_index];
            set => quad[_index] = value;
        }
        public HexCoord this[EQuadCorner _corner] => quad[_corner];
        public HexCoord B => quad.B;
        public HexCoord L => quad.L;
        public HexCoord F => quad.F;
        public HexCoord R => quad.R;
        public bool Equals(HexQuad other) => quad.Equals(other.quad);
        public IEnumerator<HexCoord> GetEnumerator() => quad.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;

namespace PCG
{
    using static PCGDefines<int>;
    public static class UGrid
    {
        public static Vector3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }
        public static GQuad ConstructGeometry(this PolyQuad _quad,Coord _offset, int[] indexes,EQuadGeometry _geometry)
        {
            var cdOS0 = _quad.m_CoordWS[indexes[0]] - _offset;
            var cdOS1 = _quad.m_CoordWS[indexes[1]] - _offset;
            var cdOS2 = _quad.m_CoordWS[indexes[2]] - _offset;
            var cdOS3 = _quad.m_CoordWS[indexes[3]] - _offset;

            if (_geometry == EQuadGeometry.Half)
            {
                var cdOS0123 = _quad.m_CenterWS - _offset;
                var posOS0 = cdOS0.ToPosition();
                var posOS1 = ((cdOS0 + cdOS1) / 2).ToPosition();
                var posOS2 = cdOS0123.ToPosition();
                var posOS3 = ((cdOS3 + cdOS0) / 2).ToPosition();
                return new GQuad(posOS0, posOS1, posOS2, posOS3);
            }
            return new GQuad(cdOS0.ToPosition(), cdOS1.ToPosition(), cdOS2.ToPosition(), cdOS3.ToPosition());
        }
    }
    
}
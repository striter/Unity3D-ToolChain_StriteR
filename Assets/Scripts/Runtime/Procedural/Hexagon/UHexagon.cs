using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Procedural.Hexagon
{
    #region Flat&Pointy
    using static HexagonConst;
    public static class HexagonConst
    {
        public static readonly float C_SQRT3 = Mathf.Sqrt(3);
        public static readonly float C_SQRT3Half = C_SQRT3 / 2f;
        public static readonly float C_Inv_SQRT3 = 1f / C_SQRT3;
    }

    public interface IHexagonShape
    {
        Coord[] m_PointOffsets { get; }
        Matrix2x2 m_AxialToPixel { get; }
        Matrix2x2 m_PixelToAxial { get; }
    }


    internal sealed class UHexagonFlatHelper : IHexagonShape
    {
        private static readonly Matrix2x2 C_AxialToPixel_Flat = new Matrix2x2( 1.5f, 0f, C_SQRT3Half, C_SQRT3);
        private static readonly Matrix2x2 C_PixelToAxial_Flat = new Matrix2x2(2f / 3f, 0, -1f / 3f, C_Inv_SQRT3);

        private static readonly Coord[] C_FlatOffsets =
        {
            new Coord(1, 0), new Coord(.5f, -C_SQRT3Half),
            new Coord(-.5f, -C_SQRT3Half), new Coord(-1, 0),
            new Coord(-.5f, C_SQRT3Half), new Coord(.5f, C_SQRT3Half)
        };

        public Coord[] m_PointOffsets => C_FlatOffsets;
        public Matrix2x2 m_AxialToPixel => C_AxialToPixel_Flat;
        public Matrix2x2 m_PixelToAxial => C_PixelToAxial_Flat;
    }

    internal sealed class UHexagonPointyHelper : IHexagonShape
    {
        private static readonly Matrix2x2 C_AxialToPixel_Pointy = new Matrix2x2(C_SQRT3, C_SQRT3Half, 0, 1.5f);
        private static readonly Matrix2x2 C_PixelToAxial_Pointy = new Matrix2x2(C_Inv_SQRT3, -1f / 3f, 0f, 2f / 3f);

        private static readonly Coord[] C_PointyOffsets =
        {
            new Coord(0, 1), new Coord(C_SQRT3Half, .5f),
            new Coord(C_SQRT3Half, -.5f), new Coord(0, -1),
            new Coord(-C_SQRT3Half, -.5f), new Coord(-C_SQRT3Half, .5f)
        };

        public Coord[] m_PointOffsets => C_PointyOffsets;
        public Matrix2x2 m_AxialToPixel => C_AxialToPixel_Pointy;
        public Matrix2x2 m_PixelToAxial => C_PixelToAxial_Pointy;
    }
    #endregion

    public static class UHexagon
    {
        private static readonly UHexagonFlatHelper m_FlatHelper = new UHexagonFlatHelper();
        private static readonly UHexagonPointyHelper m_PointHelper = new UHexagonPointyHelper();
        static IHexagonShape m_Shaper = m_FlatHelper;

        public static bool flat
        {
            set => m_Shaper = value ? (IHexagonShape) m_FlatHelper : m_PointHelper;
        }

        public static Coord[] GetHexagonPoints() => m_Shaper.m_PointOffsets;

        #region Transformation

        public static Coord ToPixel(this HexagonCoordO _offset, bool _flat)
        {
            if (_flat)
                return new Coord(_offset.col * 1.5f, C_SQRT3Half * (_offset.row * 2 + _offset.col % 2));
            return new Coord(C_SQRT3Half * (_offset.col * 2 + _offset.row % 2), _offset.row * 1.5f);
        }

        public static Coord ToPixel(this HexagonCoordA _axial) =>
            new Coord(m_Shaper.m_AxialToPixel.Multiply(_axial.col, _axial.row));

        public static Coord ToPixel(this HexagonCoordC _cube) => _cube.ToAxial().ToPixel();
        public static HexagonCoordA ToAxial(this Coord pPoint) =>
            new HexagonCoordA(m_Shaper.m_PixelToAxial.Multiply(pPoint));

        public static HexagonCoordA ToAxial(this HexagonCoordC _cube) => new HexagonCoordA(_cube.x, _cube.z);

        public static HexagonCoordC ToCube(this HexagonCoordA _axial) =>
            new HexagonCoordC(_axial.col, -_axial.col - _axial.row, _axial.row);

        #endregion

        public static Coord SetCol(this Coord _pPoint, float _col)
        {
            var axialPixel = new Coord(m_Shaper.m_PixelToAxial.Multiply(_pPoint)).SetY(_col);
            return new Coord(m_Shaper.m_AxialToPixel.Multiply(axialPixel));
        }

        public static Coord SetRow(this Coord _pPoint, float _row)
        {
            var axialPixel = new Coord(m_Shaper.m_PixelToAxial.Multiply(_pPoint)).SetX(_row);
            return new Coord(m_Shaper.m_AxialToPixel.Multiply(axialPixel));
        }

        public static bool InRange(this HexagonCoordC _cube, int _radius) => Mathf.Abs(_cube.x) <= _radius &&
                                                                       Mathf.Abs(_cube.y) <= _radius &&
                                                                       Mathf.Abs(_cube.z) <= _radius;

        public static bool InRange(this HexagonCoordA _axial, int _radius) => _axial.ToCube().InRange(_radius);

        public static int Distance(this HexagonCoordA _axial1, HexagonCoordA _axial2) =>
            Distance((HexagonCoordC) _axial1, (HexagonCoordC) _axial2);

        public static int Distance(this HexagonCoordC _cube1, HexagonCoordC _cube2) => (Mathf.Abs(_cube1.x - _cube2.x) +
            Mathf.Abs(_cube1.y - _cube2.y) +
            Mathf.Abs(_cube1.z - _cube2.z)) / 2;

        public static HexagonCoordC Rotate(this HexagonCoordC _cube, int _60degClockWiseCount)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_60degClockWiseCount % 6)
            {
                default: return _cube;
                case 1: return new HexagonCoordC(-z, -x, -y);
                case 2: return new HexagonCoordC(y, z, x);
                case 3: return new HexagonCoordC(-x, -y, -z);
                case 4: return new HexagonCoordC(-y, -z, -x);
                case 5: return new HexagonCoordC(z, x, y);
            }
        }

        public static HexagonCoordC RotateAround(this HexagonCoordC _cube, HexagonCoordC _dst, int _60degClockWiseCount)
        {
            HexagonCoordC offset = _dst - _cube;
            return _dst + offset.Rotate(_60degClockWiseCount);
        }

        public static HexagonCoordC Reflect(this HexagonCoordC _cube, ECubicAxis _axis)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_axis)
            {
                default: throw new Exception("Invalid Axis:" + _axis);
                case ECubicAxis.X: return new HexagonCoordC(x, z, y);
                case ECubicAxis.Y: return new HexagonCoordC(z, y, x);
                case ECubicAxis.Z: return new HexagonCoordC(y, x, z);
            }
        }

        public static HexagonCoordC ReflectAround(this HexagonCoordC _cube, HexagonCoordC _dst, ECubicAxis _axis)
        {
            HexagonCoordC offset = _dst - _cube;
            return _dst + offset.Reflect(_axis);
        }

        public static HexagonCoordC RotateMirror(int _radius, int _60degClockWiseCount)
        {
            return new HexagonCoordC(2 * _radius + 1, -_radius, -_radius - 1).Rotate(_60degClockWiseCount);
        }

        public static IEnumerable<HexagonCoordC> GetCoordsInRadius(this HexagonCoordC _cube, int _radius)
        {
            foreach (var axial in _cube.ToAxial().GetCoordsInRadius(_radius))
                yield return axial.ToCube();
        }

        static readonly HexagonCoordA[] m_AxialNearbyCoords =
        {
            new HexagonCoordA(1, 0), new HexagonCoordA(1, -1), new HexagonCoordA(0, -1),
            new HexagonCoordA(-1, 0), new HexagonCoordA(-1, 1), new HexagonCoordA(0, 1)
        };

        public static IEnumerable<HexagonCoordA> GetCoordsNearby(this HexagonCoordA _axial)
        {
            foreach (HexagonCoordA nearbyCoords in m_AxialNearbyCoords)
                yield return _axial + nearbyCoords;
        }

        public static IEnumerable<HexagonCoordA> GetCoordsInRadius(this HexagonCoordA _axial, int _radius)
        {
            for (int i = -_radius; i <= _radius; i++)
            for (int j = -_radius; j <= _radius; j++)
            {
                var offset = new HexagonCoordA(i, j);
                if (!offset.InRange(_radius))
                    continue;
                yield return _axial + offset;
            }
        }

        
        //Range
        static readonly HexagonCoordC[] m_CubeNearbyCoords =
        {
            new HexagonCoordC(1, -1, 0), new HexagonCoordC(1, 0, -1), new HexagonCoordC(0, 1, -1),
            new HexagonCoordC(-1, 1, 0), new HexagonCoordC(-1, 0, 1), new HexagonCoordC(0, -1, 1)
        };

        public static IEnumerable<HexagonCoordC> GetCoordsNearby(HexagonCoordC _cube)
        {
            foreach (HexagonCoordC nearbyCoords in m_CubeNearbyCoords)
                yield return _cube + nearbyCoords;
        }

        public static HexagonCoordC GetCoordsNearby(HexagonCoordC _cube, int direction)
        {
            return _cube + m_AxialNearbyCoords[direction % 6];
        }
        public static IEnumerable<(int dir,bool first,HexagonCoordC coord)> GetCoordsRinged(this HexagonCoordC _center,int _radius)
        {
            if (_radius == 0)
                yield return (-1,true,_center);

            var ringIterate = _center + m_CubeNearbyCoords[4] * _radius;
            for(int i=0;i<6;i++)
            for (int j = 0; j < _radius; j++)
            {
                yield return (i,j==0,ringIterate);
                ringIterate += m_CubeNearbyCoords[i];
            }
        }

        public static int GetCoordsRingedCount(this HexagonCoordC _center, int _radius) => _radius==0?1:_radius * 6;
    }
}

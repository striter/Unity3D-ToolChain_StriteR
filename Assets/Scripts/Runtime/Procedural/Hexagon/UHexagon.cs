using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Procedural.Hexagon
{
    using static KMath;
    #region Flat&Pointy
    public static class KHexagon
    {
        public static readonly Matrix2x2 kFlatAxialToPixel = new Matrix2x2( 1.5f, 0f, kSQRT3Half, kSQRT3);
        public static readonly Matrix2x2 kFlatPixelToAxial = new Matrix2x2(2f / 3f, 0, -1f / 3f, kInvSQRT3);
        public static readonly Coord[] kFlatUnitPoints =
        {
            new Coord(1, 0), new Coord(.5f, -kSQRT3Half),
            new Coord(-.5f, -kSQRT3Half), new Coord(-1, 0),
            new Coord(-.5f, kSQRT3Half), new Coord(.5f, kSQRT3Half)
        };

        public static readonly Matrix2x2 kPointyAxialToPixel = new Matrix2x2(kSQRT3, kSQRT3Half, 0, 1.5f);
        public static readonly Matrix2x2 kPointyPixelToAxial = new Matrix2x2(kInvSQRT3, -1f / 3f, 0f, 2f / 3f);
        public static readonly Coord[] kPointyUnitPoints =
        {
            new Coord(0, 1), new Coord(kSQRT3Half, .5f),
            new Coord(kSQRT3Half, -.5f), new Coord(0, -1),
            new Coord(-kSQRT3Half, -.5f), new Coord(-kSQRT3Half, .5f)
        };
    }
    #endregion

    public static class UHexagon
    {
        static Matrix2x2 kAxialToPixel;
        static Matrix2x2 kPixelToAxial;
        public static Coord[] kUnitPoints;
        static UHexagon()
        {
            flat = false;
        }
        
        public static bool flat   {
            set
            {
                kUnitPoints = value?KHexagon.kFlatUnitPoints:KHexagon.kPointyUnitPoints;
                kAxialToPixel =value?KHexagon.kFlatAxialToPixel: KHexagon.kPointyAxialToPixel;
                kPixelToAxial = value?KHexagon.kFlatPixelToAxial:KHexagon.kPointyPixelToAxial;
            }
        } 
        
        #region Transformation

        public static Coord ToCoord(this HexCoordO _offset, bool _flat)
        {
            if (_flat)
                return new Coord(_offset.col * 1.5f, kSQRT3Half * (_offset.row * 2 + _offset.col % 2));
            return new Coord(kSQRT3Half * (_offset.col * 2 + _offset.row % 2), _offset.row * 1.5f);
        }

        public static Coord ToCoord(this HexCoord _axial) =>
            new Coord(kAxialToPixel.Multiply(_axial.col, _axial.row));

        public static HexCoord ToCube(this Coord _pPoint) =>
            new HexCoord(kPixelToAxial.Multiply(_pPoint));
        #endregion

        public static Coord SetCol(this Coord _pPoint, float _col)
        {
            var axialPixel = new Coord(kPixelToAxial.Multiply(_pPoint)).SetY(_col);
            return new Coord(kAxialToPixel.Multiply(axialPixel));
        }

        public static Coord SetRow(this Coord _pPoint, float _row)
        {
            var axialPixel = new Coord(kPixelToAxial.Multiply(_pPoint)).SetX(_row);
            return new Coord(kAxialToPixel.Multiply(axialPixel));
        }

        public static bool InRange(this HexCoord _cube, int _radius) => Mathf.Abs(_cube.x) <= _radius &&
                                                                       Mathf.Abs(_cube.y) <= _radius &&
                                                                       Mathf.Abs(_cube.z) <= _radius;

        public static int Distance(this HexCoord _cube1, HexCoord _cube2) => (Mathf.Abs(_cube1.x - _cube2.x) +
            Mathf.Abs(_cube1.y - _cube2.y) +
            Mathf.Abs(_cube1.z - _cube2.z)) / 2;

        public static HexCoord Rotate(this HexCoord _cube, int _60degClockWiseCount)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_60degClockWiseCount % 6)
            {
                default: return _cube;
                case 1: return new HexCoord(-z, -x, -y);
                case 2: return new HexCoord(y, z, x);
                case 3: return new HexCoord(-x, -y, -z);
                case 4: return new HexCoord(-y, -z, -x);
                case 5: return new HexCoord(z, x, y);
            }
        }

        public static HexCoord RotateAround(this HexCoord _cube, HexCoord _dst, int _60degClockWiseCount)
        {
            HexCoord offset = _dst - _cube;
            return _dst + offset.Rotate(_60degClockWiseCount);
        }

        public static HexCoord Reflect(this HexCoord _cube, ECubicAxis _axis)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_axis)
            {
                default: throw new Exception("Invalid Axis:" + _axis);
                case ECubicAxis.X: return new HexCoord(x, z, y);
                case ECubicAxis.Y: return new HexCoord(z, y, x);
                case ECubicAxis.Z: return new HexCoord(y, x, z);
            }
        }

        public static HexCoord ReflectAround(this HexCoord _cube, HexCoord _dst, ECubicAxis _axis)
        {
            HexCoord offset = _dst - _cube;
            return _dst + offset.Reflect(_axis);
        }

        public static HexCoord RotateMirror(int _radius, int _60degClockWiseCount)
        {
            return new HexCoord(2 * _radius + 1, -_radius, -_radius - 1).Rotate(_60degClockWiseCount);
        }

        public static IEnumerable<HexCoord> GetCoordsInRadius(this HexCoord _axial, int _radius,bool _rounded =false)
        {
            int sqrRadius = UMath.Pow2(_radius+1);
            for (int i = -_radius; i <= _radius; i++)
            for (int j = -_radius; j <= _radius; j++)
            {
                var coord = new HexCoord(i, j);
                if (!coord.InRange(_radius))
                    continue;
                if (_rounded && coord.x * coord.x + coord.y * coord.y + coord.z*coord.z >= sqrRadius)
                    continue;
                yield return _axial + coord;
            }
        }

        public static IEnumerable<HexCoord> GetCoordsInSize(this HexCoord _axial, int _width, int _height)
        {
            for (int i = 0; i < _width; i++)
                for (int j = 0; j < _height; j++)
                {
                    var offset = new HexCoord(i, -i - j);
                    yield return _axial + offset;
                }
        }
        
        //Range
        static readonly HexCoord[] m_CubeNearbyCoords =
        {
            new HexCoord(1, -1, 0), new HexCoord(1, 0, -1), new HexCoord(0, 1, -1),
            new HexCoord(-1, 1, 0), new HexCoord(-1, 0, 1), new HexCoord(0, -1, 1)
        };

        public static IEnumerable<HexCoord> GetCoordsNearby(this HexCoord _cube)
        {
            foreach (HexCoord nearbyCoords in m_CubeNearbyCoords)
                yield return _cube + nearbyCoords;
        }

        public static HexCoord GetCoordsNearby(this HexCoord _cube, int direction)
        {
            return _cube + m_CubeNearbyCoords[direction % 6];
        }
        public static IEnumerable<(int dir,bool first,HexCoord coord)> GetCoordsRinged(this HexCoord _center,int _radius)
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

        public static int GetCoordsRingedCount(this HexCoord _center, int _radius) => _radius==0?1:_radius * 6;
    }
}

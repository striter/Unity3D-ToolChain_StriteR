using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Hexagon
{
    #region Shape
    using static HexagonConst;
    
    public static class HexagonConst
    {
        public static readonly float C_SQRT3 = Mathf.Sqrt(3);
        public static readonly float C_SQRT3Half = C_SQRT3 / 2f;
        public static readonly float C_Inv_SQRT3 = 1f/C_SQRT3;
    }
    public interface IHexagonShape
    {
        public HexPixel[] m_PointOffsets { get; }
        public Matrix2x2 m_AxialToPixel { get; }
        public Matrix2x2 m_PixelToAxial { get; }
    }
    
    public class UHexagonPointyHelper:IHexagonShape
    {
        private static readonly Matrix2x2 C_AxialToPixel_Pointy = new Matrix2x2( C_SQRT3,C_SQRT3Half,0,1.5f);
        private static readonly Matrix2x2 C_PixelToAxial_Pointy = new Matrix2x2(C_Inv_SQRT3,-1f/3f,0f,2f/3f);
        private static readonly HexPixel[] C_PointyOffsets={
            new HexPixel(0, 1), new HexPixel(C_SQRT3Half, .5f),
            new HexPixel(C_SQRT3Half, -.5f), new HexPixel(0, -1),
            new HexPixel(-C_SQRT3Half, -.5f), new HexPixel(-C_SQRT3Half, .5f)
        };
        
        public HexPixel[] m_PointOffsets => C_PointyOffsets;
        public Matrix2x2 m_AxialToPixel => C_AxialToPixel_Pointy;
        public Matrix2x2 m_PixelToAxial => C_PixelToAxial_Pointy;
    }

    internal sealed class UHexagonFlatHelper : IHexagonShape
    {
        private static readonly Matrix2x2 C_AxialToPixel_Flat = new Matrix2x2(1.5f,0f,C_SQRT3Half,C_SQRT3);  
        private static readonly Matrix2x2 C_PixelToAxial_Flat = new Matrix2x2(2f/3f,0,-1f/3f,C_Inv_SQRT3);
        private static readonly HexPixel[] C_FlatOffsets={
            new HexPixel(1, 0), new HexPixel(.5f, -C_SQRT3Half),
            new HexPixel(-.5f, -C_SQRT3Half), new HexPixel(-1, 0),
            new HexPixel(-.5f, C_SQRT3Half), new HexPixel(.5f, C_SQRT3Half)
        };
        
        public HexPixel[] m_PointOffsets => C_FlatOffsets;
        public Matrix2x2 m_AxialToPixel => C_AxialToPixel_Flat;
        public Matrix2x2 m_PixelToAxial => C_PixelToAxial_Flat;
    }
    #endregion

    public static class UHexagon
    {
        private static readonly UHexagonFlatHelper m_FlatHelper = new UHexagonFlatHelper();
        private static readonly UHexagonPointyHelper m_PointHelper = new UHexagonPointyHelper();
        public static IHexagonShape m_Shaper { get; private set; } = m_FlatHelper;

        public static bool flat
        {
            set { m_Shaper = value ? (IHexagonShape) m_FlatHelper : m_PointHelper; }
        }

        public static HexPixel[] GetPoints() => m_Shaper.m_PointOffsets;

        public static HexPixel ToPixel(this HexOffset _offset, bool _flat)
        {
            if (_flat)
                return new HexPixel(_offset.col * 1.5f, C_SQRT3Half * (_offset.row * 2 + _offset.col % 2));
            return new HexPixel(C_SQRT3Half * (_offset.col * 2 + _offset.row % 2), _offset.row * 1.5f);
        }

        public static HexPixel ToPixel(this HexAxial _axial) =>
            new HexPixel(m_Shaper.m_AxialToPixel.Multiply(_axial.col, _axial.row));

        public static HexAxial ToAxial(this HexPixel _pixel) => new HexAxial(m_Shaper.m_PixelToAxial.Multiply(_pixel));
        public static HexAxial ToAxial(this HexCube _cube) => new HexAxial(_cube.x, _cube.z);

        public static HexCube ToCube(this HexAxial _axial) =>
            new HexCube(_axial.col, -_axial.col - _axial.row, _axial.row);


        public static HexPixel SetCol(this HexPixel _pixel, float _col)
        {
            var axialPixel = new HexPixel(m_Shaper.m_PixelToAxial.Multiply(_pixel)).SetY(_col);
            return new HexPixel(m_Shaper.m_AxialToPixel.Multiply(axialPixel));
        }

        public static HexPixel SetRow(this HexPixel _pixel, float _row)
        {
            var axialPixel = new HexPixel(m_Shaper.m_PixelToAxial.Multiply(_pixel)).SetX(_row);
            return new HexPixel(m_Shaper.m_AxialToPixel.Multiply(axialPixel));
        }

        public static bool Inside(this HexAxial _axial, int _radius) => ((HexCube) _axial).Inside(_radius);

        public static bool Inside(this HexCube _cube, int _radius) => Mathf.Abs(_cube.x) < _radius &&
                                                                      Mathf.Abs(_cube.y) < _radius &&
                                                                      Mathf.Abs(_cube.z) < _radius;

        public static int Distance(this HexAxial _axial1, HexAxial _axial2) =>
            Distance((HexCube) _axial1, (HexCube) _axial2);

        public static int Distance(this HexCube _cube1, HexCube _cube2) => (Mathf.Abs(_cube1.x - _cube2.x) +
                                                                            Mathf.Abs(_cube1.y - _cube2.y) +
                                                                            Mathf.Abs(_cube1.z - _cube2.z)) / 2;

        public static HexCube Rotate(this HexCube _cube, int _60degClockWiseCount)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_60degClockWiseCount % 6)
            {
                default: return _cube;
                case 1: return new HexCube(-z, -x, -y);
                case 2: return new HexCube(y, z, x);
                case 3: return new HexCube(-x, -y, -z);
                case 4: return new HexCube(-y, -z, -x);
                case 5: return new HexCube(z, x, y);
            }
        }

        public static HexCube RotateAround(this HexCube _cube, HexCube _dst, int _60degClockWiseCount)
        {
            HexCube offset = _dst - _cube;
            return _dst + offset.Rotate(_60degClockWiseCount);
        }

        public static HexCube Reflect(this HexCube _cube, ECubeAxis _axis)
        {
            var x = _cube.x;
            var y = _cube.y;
            var z = _cube.z;
            switch (_axis)
            {
                default: throw new Exception("Invalid Axis:" + _axis);
                case ECubeAxis.X: return new HexCube(x, z, y);
                case ECubeAxis.Y: return new HexCube(z, y, x);
                case ECubeAxis.Z: return new HexCube(y, x, z);
            }
        }

        public static HexCube ReflectAround(this HexCube _cube, HexCube _dst, ECubeAxis _axis)
        {
            HexCube offset = _dst - _cube;
            return _dst + offset.Reflect(_axis);
        }

        public static HexCube RotateMirror(int _radius, int _60degClockWiseCount)
        {
            return new HexCube(2 * _radius + 1, -_radius, -_radius - 1).Rotate(_60degClockWiseCount);
        }

        public static HexCube CellToArea(HexCube _cell, int _areaRadius,out int xh,out int yh,out int zh)
        {
            var x = _cell.x;
            var y = _cell.y;
            var z = _cell.z;
            float area = 3f * _areaRadius * _areaRadius + 3f * _areaRadius + 1f;
            float shift = 3f * _areaRadius + 2f;
            xh = Mathf.FloorToInt((y + shift * x) / area);
            yh = Mathf.FloorToInt((z + shift * y) / area);
            zh = Mathf.FloorToInt((x + shift * z) / area);
            float i = Mathf.FloorToInt((1 + xh - yh) / 3f);
            float j = Mathf.FloorToInt((1 + yh - zh) / 3f);
            float k = Mathf.FloorToInt((1 + zh - xh) / 3f);
            return new HexCube(i, j, k);
        }
        public static HexCube CellToArea(HexCube _cell, int _areaRadius) => CellToArea(_cell,_areaRadius,out int xh,out int yh,out int zh);
    }
}
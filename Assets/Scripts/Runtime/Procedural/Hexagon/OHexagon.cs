using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using OSwizzling;
using UnityEngine;
namespace Procedural.Hexagon
{
    public enum ECubicAxis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public struct HexagonCoordC:IEquatable<HexagonCoordC>,IEqualityComparer<HexagonCoordC>  //Cubic
    {
        public static HexagonCoordC zero = new HexagonCoordC(0, 0, 0);
        public int x;
        public int y;
        public int z;

        public HexagonCoordC(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public HexagonCoordC(float _x, float _y, float _z)
        {
            var rx = Mathf.RoundToInt(_x);
            var ry = Mathf.RoundToInt(_y);
            var rz = Mathf.RoundToInt(_z);

            var xDiff = Mathf.Abs(rx - _x);
            var yDiff = Mathf.Abs(ry - _y);
            var zDiff = Mathf.Abs(rz - _z);

            if (xDiff > yDiff && xDiff > zDiff)
                rx = -ry - rz;
            else if (yDiff > zDiff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            x = rx;
            y = ry;
            z = rz;
        }

        public static HexagonCoordC operator +(HexagonCoordC _hex1, HexagonCoordC _hex2) =>
            new HexagonCoordC(_hex1.x + _hex2.x, _hex1.y + _hex2.y, _hex1.z + _hex2.z);

        public static HexagonCoordC operator -(HexagonCoordC _hex1, HexagonCoordC _hex2) =>
            new HexagonCoordC(_hex1.x - _hex2.x, _hex1.y - _hex2.y, _hex1.z - _hex2.z);
        
        public static HexagonCoordC operator *(HexagonCoordC _hex1, int _value) =>
            new HexagonCoordC(_hex1.x * _value, _hex1.y *_value, _hex1.z * _value);
        
        public static HexagonCoordC operator %(HexagonCoordC _hex1, int _value) =>
            new HexagonCoordC(_hex1.x % _value, _hex1.y % _value, _hex1.z % _value);
        
        public static HexagonCoordC operator /(HexagonCoordC _hex1, int _value) =>
            new HexagonCoordC(_hex1.x / _value, _hex1.y / _value, _hex1.z / _value);

        public static HexagonCoordC operator -(HexagonCoordC _hex1) => new HexagonCoordC(-_hex1.x, -_hex1.y, -_hex1.z);

        public static bool operator ==(HexagonCoordC offset1, HexagonCoordC offset2) =>
            offset1.x == offset2.x && offset1.y == offset2.y && offset1.z == offset2.z;

        public static bool operator !=(HexagonCoordC offset1, HexagonCoordC offset2) =>
            offset1.x != offset2.x || offset1.y != offset2.y || offset1.z != offset2.z;

        public static explicit operator HexagonCoordA(HexagonCoordC cube) => cube.ToAxial();


        public override string ToString() => $"{x},{y},{z}";
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
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ z;
                return hashCode;
            }
        }
        public bool Equals(HexagonCoordC _src, HexagonCoordC _dst)=> _src.x == _dst.x && _src.y == _dst.y && _src.z == _dst.z;
        public bool Equals(HexagonCoordC _dst)=> x == _dst.x && y == _dst.y && z == _dst.z;
        public override bool Equals(object obj)=> obj is HexagonCoordC other && Equals(other);
    }
    
    [Serializable]
    public struct HexagonCoordA //Axial
    {
        public static HexagonCoordA zero = new HexagonCoordA(0, 0);
        public int col;
        public int row;

        public HexagonCoordA(int _col, int _row)
        {
            col = _col;
            row = _row;
        }

        public HexagonCoordA SetCol(int _col)
        {
            col = _col;
            return this;
        }

        public HexagonCoordA SetRow(int _row)
        {
            row = _row;
            return this;
        }

        public HexagonCoordA((float, float) _float2) : this(Mathf.RoundToInt(_float2.Item1),
            Mathf.RoundToInt(_float2.Item2))
        {
        }

        public static HexagonCoordA operator +(HexagonCoordA _hex1, HexagonCoordA _hex2) =>
            new HexagonCoordA(_hex1.col + _hex2.col, _hex1.row + _hex2.row);

        public static HexagonCoordA operator -(HexagonCoordA _hex1, HexagonCoordA _hex2) =>
            new HexagonCoordA(_hex1.col - _hex2.col, _hex1.row - _hex2.row);

        public static HexagonCoordA operator -(HexagonCoordA pHex) => new HexagonCoordA(-pHex.col, -pHex.row);

        public static bool operator ==(HexagonCoordA _hex1, HexagonCoordA _hex2) =>
            _hex1.col == _hex2.col && _hex1.row == _hex2.row;

        public static bool operator !=(HexagonCoordA _hex1, HexagonCoordA _hex2) =>
            _hex1.col != _hex2.col || _hex1.row != _hex2.row;

        public static implicit operator HexagonCoordC(HexagonCoordA _axial) => _axial.ToCube();
        public bool Equals(HexagonCoordA other) => col == other.col && row == other.row;
        public override bool Equals(object obj) => obj is HexagonCoordA other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }

        public override string ToString() => $"{col},{row}";

    }
    
    [Serializable]
    public struct HexagonCoordO //Offset
    {
        public int col;
        public int row;

        public HexagonCoordO(int _col, int _row)
        {
            col = _col;
            row = _row;
        }

        public static bool operator ==(HexagonCoordO _hex1, HexagonCoordO _hex2) =>
            _hex1.col == _hex2.col && _hex1.row == _hex2.row;

        public static bool operator !=(HexagonCoordO _hex1, HexagonCoordO _hex2) =>
            _hex1.col != _hex2.col || _hex1.row != _hex2.row;

        public bool Equals(HexagonCoordO other) => col == other.col && row == other.row;

        public override bool Equals(object obj) => obj is HexagonCoordO other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }
    }
}
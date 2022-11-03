using System;
using System.Collections.Generic;
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
    public struct HexCoord:IEquatable<HexCoord>,IEqualityComparer<HexCoord>  //Cubic
    {
        public static HexCoord zero = new HexCoord(0, 0, 0);
        public int x;
        public int y;
        public int z;
        public int col => x;
        public int row => z;

        public HexCoord(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public HexCoord(int _col, int _row)
        {
            x = _col;
            z = _row;
            y = -x - z;
        }
        public HexCoord((float _col, float _row) tuple):this(Mathf.RoundToInt(tuple._col),Mathf.RoundToInt(tuple._row)) { }
        public HexCoord(float _x, float _y, float _z)
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

        public static HexCoord operator +(HexCoord _hex1, HexCoord _hex2)
        {
            ref var hex = ref _hex1;
            hex.x += _hex2.x;
            hex.y += _hex2.y;
            hex.z += _hex2.z;
            return hex;
        }
        
        public static HexCoord operator -(HexCoord _hex1, HexCoord _hex2)
        {
            ref var hex = ref _hex1;
            hex.x -= _hex2.x;
            hex.y -= _hex2.y;
            hex.z -= _hex2.z;
            return hex;
        }
        
        public static HexCoord operator *(HexCoord _hex1, int _value)
        {
            ref var hex = ref _hex1;
            hex.x *= _value;
            hex.y *= _value;
            hex.z *= _value;
            return hex;
        }
        
        public static HexCoord operator %(HexCoord _hex1, int _value)
        {
            ref var hex = ref _hex1;
            hex.x %= _value;
            hex.y %= _value;
            hex.z %= _value;
            return hex;
        }

        public static HexCoord operator /(HexCoord _hex1, int _value)
        {
            ref var hex = ref _hex1;
            hex.x /= _value;
            hex.y /= _value;
            hex.z /= _value;
            return hex;
        }

        public static HexCoord operator -(HexCoord _hex1)
        {
            ref var hex = ref _hex1;
            hex.x = -_hex1.x;
            hex.y = -_hex1.y;
            hex.z = -_hex1.z;
            return hex;
        }

        public static bool operator ==(HexCoord offset1, HexCoord offset2) =>
            offset1.x == offset2.x && offset1.y == offset2.y && offset1.z == offset2.z;

        public static bool operator !=(HexCoord offset1, HexCoord offset2) =>
            offset1.x != offset2.x || offset1.y != offset2.y || offset1.z != offset2.z;

        public override string ToString() => $"{x},{y},{z}";
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
        public bool Equals(HexCoord _src, HexCoord _dst)=> _src.x == _dst.x && _src.y == _dst.y && _src.z == _dst.z;
        public bool Equals(HexCoord _dst)=> x == _dst.x && y == _dst.y && z == _dst.z;
        public override bool Equals(object obj)=> obj is HexCoord other && Equals(other);
    }
    
    [Serializable]
    public struct HexCoordO //Offset
    {
        public int col;
        public int row;

        public HexCoordO(int _col, int _row)
        {
            col = _col;
            row = _row;
        }

        public static bool operator ==(HexCoordO _hex1, HexCoordO _hex2) =>
            _hex1.col == _hex2.col && _hex1.row == _hex2.row;

        public static bool operator !=(HexCoordO _hex1, HexCoordO _hex2) =>
            _hex1.col != _hex2.col || _hex1.row != _hex2.row;

        public bool Equals(HexCoordO other) => col == other.col && row == other.row;

        public override bool Equals(object obj) => obj is HexCoordO other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }
    }
}
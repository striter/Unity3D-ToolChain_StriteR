using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using OSwizzling;
using UnityEngine;
namespace Procedural.Hexagon
{
    public enum ECubeAxis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public struct PHexAxial
    {
        public static PHexAxial zero = new PHexAxial(0, 0);
        public int col;
        public int row;

        public PHexAxial(int _col, int _row)
        {
            col = _col;
            row = _row;
        }

        public PHexAxial SetCol(int _col)
        {
            col = _col;
            return this;
        }

        public PHexAxial SetRow(int _row)
        {
            row = _row;
            return this;
        }

        public PHexAxial((float, float) _float2) : this(Mathf.RoundToInt(_float2.Item1),
            Mathf.RoundToInt(_float2.Item2))
        {
        }

        public static PHexAxial operator +(PHexAxial _hex1, PHexAxial _hex2) =>
            new PHexAxial(_hex1.col + _hex2.col, _hex1.row + _hex2.row);

        public static PHexAxial operator -(PHexAxial _hex1, PHexAxial _hex2) =>
            new PHexAxial(_hex1.col - _hex2.col, _hex1.row - _hex2.row);

        public static PHexAxial operator -(PHexAxial pHex) => new PHexAxial(-pHex.col, -pHex.row);

        public static bool operator ==(PHexAxial _hex1, PHexAxial _hex2) =>
            _hex1.col == _hex2.col && _hex1.row == _hex2.row;

        public static bool operator !=(PHexAxial _hex1, PHexAxial _hex2) =>
            _hex1.col != _hex2.col || _hex1.row != _hex2.row;

        public static implicit operator PHexCube(PHexAxial _axial) => _axial.ToCube();
        public bool Equals(PHexAxial other) => col == other.col && row == other.row;
        public override bool Equals(object obj) => obj is PHexAxial other && Equals(other);

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
    public struct PHexCube
    {
        public static PHexCube zero = new PHexCube(0, 0, 0);
        public int x;
        public int y;
        public int z;

        public PHexCube(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public PHexCube(float _x, float _y, float _z)
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

        public static PHexCube operator +(PHexCube _hex1, PHexCube _hex2) =>
            new PHexCube(_hex1.x + _hex2.x, _hex1.y + _hex2.y, _hex1.z + _hex2.z);

        public static PHexCube operator -(PHexCube _hex1, PHexCube _hex2) =>
            new PHexCube(_hex1.x - _hex2.x, _hex1.y - _hex2.y, _hex1.z - _hex2.z);
        
        public static PHexCube operator *(PHexCube _hex1, int _value) =>
            new PHexCube(_hex1.x * _value, _hex1.y *_value, _hex1.z * _value);
        
        public static PHexCube operator %(PHexCube _hex1, int _value) =>
            new PHexCube(_hex1.x % _value, _hex1.y % _value, _hex1.z % _value);
        
        public static PHexCube operator /(PHexCube _hex1, int _value) =>
            new PHexCube(_hex1.x / _value, _hex1.y / _value, _hex1.z / _value);

        public static PHexCube operator -(PHexCube _hex1) => new PHexCube(-_hex1.x, -_hex1.y, -_hex1.z);

        public static bool operator ==(PHexCube offset1, PHexCube offset2) =>
            offset1.x == offset2.x && offset1.y == offset2.y && offset1.z == offset2.z;

        public static bool operator !=(PHexCube offset1, PHexCube offset2) =>
            offset1.x != offset2.x || offset1.y != offset2.y || offset1.z != offset2.z;

        public bool Equals(PHexCube other) => x == other.x && y == other.y && z == other.z;
        public static explicit operator PHexAxial(PHexCube cube) => cube.ToAxial();
        public override bool Equals(object obj) => obj is PHexCube other && Equals(other);

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

        public override string ToString() => $"{x},{y},{z}";
    }
    
    [Serializable]
    public struct PHexOffset
    {
        public int col;
        public int row;

        public PHexOffset(int _col, int _row)
        {
            col = _col;
            row = _row;
        }

        public static bool operator ==(PHexOffset _hex1, PHexOffset _hex2) =>
            _hex1.col == _hex2.col && _hex1.row == _hex2.row;

        public static bool operator !=(PHexOffset _hex1, PHexOffset _hex2) =>
            _hex1.col != _hex2.col || _hex1.row != _hex2.row;

        public bool Equals(PHexOffset other) => col == other.col && row == other.row;

        public override bool Equals(object obj) => obj is PHexOffset other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }
    }
}
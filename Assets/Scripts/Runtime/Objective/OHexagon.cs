using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using OSwizzling;
using UnityEngine;

namespace Hexagon
{
    public enum EHexagonAxis
    {
        Offset,
        Axial,
        Cube,
    }

    public enum ECubeAxis
    {
        X,
        Y,
        Z,
    }
    [Serializable]
    public struct HexPixel
    {
        public float x;
        public float y;
        public HexPixel SetX(float _x)
        {
            x = _x;
            return this;
        }
        public HexPixel SetY(float _y)
        {
            y = _y;
            return this;
        }

        public HexPixel(float _x,float _y)  { x = _x; y = _y; }
        public HexPixel((float, float) _pixels):this(_pixels.Item1,_pixels.Item2)  {   }
        public static HexPixel operator *(HexPixel _hex, float _scale)=> new HexPixel(_hex.x*_scale,_hex.y*_scale);
        public static implicit operator (float, float)(HexPixel _hex) => (_hex.x, _hex.y);
        public override string ToString() => $"Hex(Pixel):{x:F1},{y:F1}";
    }
    
    [Serializable]
    public struct HexOffset
    {
        public int col;
        public int row;
        public HexOffset(int _col, int _row) { col = _col; row = _row; }
        public static bool operator ==(HexOffset _hex1, HexOffset _hex2)=> _hex1.col == _hex2.col && _hex1.row == _hex2.row;
        public static bool operator !=(HexOffset _hex1, HexOffset _hex2)=> _hex1.col != _hex2.col || _hex1.row != _hex2.row;
        
        public bool Equals(HexOffset other)=>col == other.col && row == other.row;

        public override bool Equals(object obj)=> obj is HexOffset other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }
    }
    
    [Serializable]
    public struct HexAxial
    {
        public static HexAxial zero = new HexAxial(0, 0);
        public int col;
        public int row;
        public HexAxial(int _col, int _row) { col = _col; row = _row; }
        public HexAxial SetCol(int _col)
        {
            col = _col;
            return this;
        }
        public HexAxial SetRow(int _row)
        {
            row = _row;
            return this;
        }
        
        public HexAxial((float,float) _float2):this(Mathf.RoundToInt(_float2.Item1),Mathf.RoundToInt( _float2.Item2)){}
        public static HexAxial operator +(HexAxial _hex1, HexAxial _hex2)=>new HexAxial(_hex1.col+_hex2.col,_hex1.row+_hex2.row);
        public static HexAxial operator -(HexAxial _hex1, HexAxial _hex2)=>new HexAxial(_hex1.col-_hex2.col,_hex1.row-_hex2.row);
        public static HexAxial operator -(HexAxial _hex)=>new HexAxial(-_hex.col,-_hex.row);
        public static bool operator ==(HexAxial _hex1, HexAxial _hex2)=> _hex1.col == _hex2.col && _hex1.row == _hex2.row;
        public static bool operator !=(HexAxial _hex1, HexAxial _hex2)=> _hex1.col != _hex2.col || _hex1.row != _hex2.row;
        public static explicit operator HexCube(HexAxial _axial) => _axial.ToCube();       
        public bool Equals(HexAxial other)=> col == other.col && row == other.row;
        public override bool Equals(object obj)=>obj is HexAxial other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                return (col * 397) ^ row;
            }
        }

        public override string ToString()=> $"Hex(Axial):{col},{row}";

        public static HexAxial[] m_NearbyCoords ={
            new HexAxial(1,0),new HexAxial(1,-1),new HexAxial(0,-1),
            new HexAxial(-1,0),new HexAxial(-1,1),new HexAxial(0,1)
        };
    }

    [Serializable]
    public struct HexCube
    {
        public static HexCube zero = new HexCube(0, 0,0);
        public int x;
        public int y;
        public int z;
        public HexCube(int _x, int _y, int _z)  {  x = _x;  y = _y;  z = _z; }

        public HexCube(float _x, float _y, float _z)
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
        public static HexCube operator +(HexCube _hex1, HexCube _hex2)=>new HexCube(_hex1.x+_hex2.x,_hex1.y+_hex2.y,_hex1.z+_hex2.z);
        public static HexCube operator -(HexCube _hex1, HexCube _hex2)=>new HexCube(_hex1.x-_hex2.x,_hex1.y-_hex2.y,_hex1.z-_hex2.z);
        public static HexCube operator -(HexCube _hex1)=>new HexCube(-_hex1.x,-_hex1.y,-_hex1.z);
        
        public static bool operator ==(HexCube offset1, HexCube offset2)=> offset1.x == offset2.x && offset1.y == offset2.y&&offset1.z==offset2.z;
        public static bool operator !=(HexCube offset1, HexCube offset2)=> offset1.x != offset2.x || offset1.y !=offset2.y||offset1.z !=offset2.z;
        public bool Equals(HexCube other)=> x == other.x && y == other.y && z == other.z;
        public static explicit operator HexAxial(HexCube cube) => cube.ToAxial() ;
        public override bool Equals(object obj)=> obj is HexCube other && Equals(other);
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
        public override string ToString()=> $"Hex(Cube):{x},{y},{z}";

        public static HexCube[] m_NearbyCoords = {
            new HexCube(1,-1,0),new HexCube(1,0,-1),new HexCube(0,1,-1),
            new HexCube(-1,1,0),new HexCube(-1,0,1),new HexCube(0,-1,1)
        };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    [Serializable]
    public struct Coord
    {
        public float x;
        public float y;

        public Coord SetX(float _x)
        {
            x = _x;
            return this;
        }

        public Coord SetY(float _y)
        {
            y = _y;
            return this;
        }

        public Coord(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public Coord((float x, float y) _axial) : this(_axial.x, _axial.y)
        {
        }

        public static Coord operator +(Coord _a,Coord _b)=>new Coord(_a.x+_b.x,_a.y+_b.y);
        public static Coord operator -(Coord _a,Coord _b)=>new Coord(_a.x-_b.x,_a.y-_b.y);
        public static Coord operator *(Coord _a,Coord _b)=>new Coord(_a.x*_b.x,_a.y*_b.y);
        public static Coord operator /(Coord _a,Coord _b)=>new Coord(_a.x/_b.x,_a.y/_b.y);
        public static Coord operator *(Coord _c, float _scale) => new Coord(_c.x * _scale, _c.y * _scale);
        public static Coord operator /(Coord _c,float _div)=>new Coord(_c.x/_div,_c.y/_div);
        
        public static implicit operator (float x, float y)(Coord _pos) => (_pos.x, _pos.y);
        public static implicit operator Coord( (float x, float y) _pos) => new Coord(_pos.x, _pos.y);
        public static implicit operator Vector2(Coord _pos) => new Vector2(_pos.x, _pos.y);
        public static implicit operator Coord(Vector2 _pos) => new Coord(_pos.x, _pos.y);
        public override string ToString() => $"Hex(Pixel):{x:F1},{y:F1}";
        public static readonly Coord zero = new Coord(0, 0);
        public static readonly Coord one = new Coord(1, 1);

        public static Coord Normalize(Coord _src)
        {
            var length = Mathf.Sqrt(_src.x*_src.x+_src.y*_src.y);
            return new Coord(_src.x/length,_src.y/length);
        }
    }

}

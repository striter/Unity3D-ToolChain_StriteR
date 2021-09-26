using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
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

        public static Coord operator +(Coord _a, Coord _b)
        {
            ref var coord = ref _a;
            coord.x += _b.x;
            coord.y += _b.y;
            return coord;
        }
        public static Coord operator *(Coord _a, Coord _b)
        {
            ref var coord = ref _a;
            coord.x *= _b.x;
            coord.y *= _b.y;
            return coord;
        }
        public static Coord operator -(Coord _a, Coord _b)
        {
            ref var coord = ref _a;
            coord.x -= _b.x;
            coord.y -= _b.y;
            return coord;
        }
        public static Coord operator /(Coord _a, Coord _b)
        {
            ref var coord = ref _a;
            coord.x /= _b.x;
            coord.y /= _b.y;
            return coord;
        }
        public static Coord operator *(Coord _c, float _scale)
        {
            ref var coord = ref _c;
            coord.x *= _scale;
            coord.y *= _scale;
            return coord;
        }
        public static Coord operator /(Coord _c, float _scale)
        {
            ref var coord = ref _c;
            coord.x /= _scale;
            coord.y /= _scale;
            return coord;
        }
        public static Coord Normalize(Coord _src)=>_src/_src.magnitude;
        public float sqrMagnitude => x * x + y * y;
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        
        
        public static implicit operator (float x, float y)(Coord _pos) => (_pos.x, _pos.y);
        public static implicit operator Coord( (float x, float y) _pos) => new Coord(_pos.x, _pos.y);
        public static implicit operator Vector2(Coord _pos) => new Vector2(_pos.x, _pos.y);
        public static implicit operator Coord(Vector2 _pos) => new Coord(_pos.x, _pos.y);
        public override string ToString() => $"{x:F1},{y:F1}";
        public static readonly Coord zero = new Coord(0, 0);
        public static readonly Coord one = new Coord(1, 1);

        public static Coord operator *(Quaternion rotation, Coord point)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num12 = rotation.w * num3;
            Coord coord;
            coord.x = (1.0f - (num5 + num6)) *  point.x + ( num7 -  num12) *  point.y;
            coord.y = (num7 +  num12) * point.x + (1.0f - ( num4 +  num6)) *  point.y;
            return coord;
        }
    }
}

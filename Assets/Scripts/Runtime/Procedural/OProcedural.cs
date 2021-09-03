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
        public override string ToString() => $"Hex(Pixel):{x:F1},{y:F1}";
        public static readonly Coord zero = new Coord(0, 0);
        public static readonly Coord one = new Coord(1, 1);

    }

    [Serializable]
    public struct CoordQuad : IQuad<Coord>,IEnumerable<Coord>
    {
        public Coord vertex0 { get; set; }
        public Coord vertex1 { get; set; }
        public Coord vertex2 { get; set; }
        public Coord vertex3 { get; set; }
        public Coord this[int index]
        {
            get
            {
                switch (index)
                {
                    default: throw new Exception("Invalid Index:"+index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                    case 3: return vertex3;
                }
            }
        }

        public CoordQuad(Coord _vertex0,Coord _vertex1,Coord _vertex2,Coord _vertex3)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertex3 = _vertex3;
        }

        public IEnumerator<Coord> GetEnumerator()
        {
            yield return vertex0;
            yield return vertex1;
            yield return vertex2;
            yield return vertex3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

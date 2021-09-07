using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public static class UProcedural
    {
        public static Coord Lerp(this Coord _src, Coord _dst, float _value)
        {
            return _src + (_dst-_src) * _value;
        }
    }
}
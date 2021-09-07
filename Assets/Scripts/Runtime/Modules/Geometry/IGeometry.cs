using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public interface IQuad<T> where T:struct
    {
        T vertex0 { get; set; }
        T vertex1 { get; set; }
        T vertex2 { get; set; }
        T vertex3 { get; set; }
        T this[int index] { get; }
    }
    public interface ITriangle<T> where T:struct
    {
        T vertex0 { get; set; }
        T vertex1 { get; set; }
        T vertex2 { get; set; }
        T this[int index] { get; }
    }
}
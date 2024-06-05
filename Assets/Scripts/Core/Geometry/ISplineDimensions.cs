using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    public interface ISplineDimensions<T> : ICurveDimensions<T>
    {
        IEnumerable<T> Coordinates { get; }
    }

}